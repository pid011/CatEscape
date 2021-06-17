using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;
using Serilog;

namespace CatEscape.Server
{
    public class GameServer
    {
        public const int MaxPlayers = 2;
        public const float Timeout = 5f;

        private readonly UdpClient _server;
        private readonly ConcurrentDictionary<int, ServerPlayer> _players = new();
        private readonly ConcurrentQueue<(IPEndPoint, IPacket)> _packets = new();
        private readonly object _socketLock = new();

        public event EventHandler<PlayerEventArgs> PlayerJoin;
        public event EventHandler<PlayerEventArgs> PlayerDisconnect;

        public IReadOnlyDictionary<int, ServerPlayer> Players => _players;

        public int Port { get; }

        public GameServer(int port)
        {
            Port = port;
            _server = new UdpClient(port);
        }

        public async Task RunAsync()
        {
            var listeningTask = ListeningPacketsAsync();
            var sendingTask = SendPacketsInQueueAsync();
            var timeoutTask = DisconnectingTimeoutPlayersAsync();

            Log.Information($"Server is opened at port [{Port}]");
            await Task.WhenAll(listeningTask, sendingTask, timeoutTask);
        }

        private async Task ListeningPacketsAsync()
        {
            while (true)
            {
                var (receivedPacket, sender) = await ReceivePacketAsync();
                Log.Debug($"Packet received: endpoint - {sender}, id - {receivedPacket.Id}, type: {receivedPacket.Type}");

                if (receivedPacket.Type == PacketType.Connect)
                {
                    var newPlayer = new ServerPlayer(receivedPacket.Id, receivedPacket.Name, sender);
                    var reply = new ReplyPacket();

                    if (_players.Count == MaxPlayers)
                    {
                        reply.Reason = ReplyPacket.Reasons.ServerIsFull;
                    }
                    else if (!_players.TryAdd(newPlayer.Id, newPlayer))
                    {
                        reply.Reason = ReplyPacket.Reasons.NameOfPlayerIsAlreadyConnected;
                    }
                    else
                    {
                        if (_players.Count == 1)
                        {
                            newPlayer.IsHost = true;
                        }

                        reply.Result = true;
                        reply.Reason = ReplyPacket.Reasons.None;
                        Log.Information(
                            $"Connected player: endpoint - {newPlayer.RemoteEndPoint}, id - {newPlayer.Id}, name: {newPlayer.Name}");
                    }

                    SendPacket(newPlayer.RemoteEndPoint, reply);
                }
                else if (_players.TryGetValue(receivedPacket.Id, out var targetPlayer))
                {
                    if (receivedPacket is GamePacket gamePacket)
                    {
                        targetPlayer.SyncData(gamePacket);
                    }

                    if (receivedPacket.Type == PacketType.CheckSignal)
                    {
                        targetPlayer.LastResponded = DateTime.Now;
                        continue;
                    }

                    if (receivedPacket.Type == PacketType.Disconnect)
                    {
                        if (!_players.TryRemove(receivedPacket.Id, out var removedPlayer))
                        {
                            continue;
                        }
                        var disconnectPacket = new InfoPacket
                        {
                            Type = PacketType.Disconnect,
                            Id = removedPlayer.Id
                        };

                        SendPacketToAll(disconnectPacket);
                        Log.Information(
                            $"Disconnected player: endpoint - {removedPlayer.RemoteEndPoint}, id - {removedPlayer.Id}, name: {removedPlayer.Name}");
                    }

                    if (receivedPacket.Type == PacketType.Ready)
                    {
                        foreach (var (id, player) in _players)
                        {
                            SendPacket(targetPlayer.RemoteEndPoint, new InfoPacket
                            {
                                Type = PacketType.PlayerJoin,
                                Id = player.Id,
                                Name = player.Name,
                                IsHost = player.IsHost
                            });
                        }

                        SendPacketToAll(new InfoPacket
                        {
                            Type = PacketType.PlayerJoin,
                            Id = targetPlayer.Id,
                            Name = targetPlayer.Name,
                            IsHost = targetPlayer.IsHost
                        }, excludeId: targetPlayer.Id);

                        continue;
                    }

                    SendPacketToAll(receivedPacket, isEcho: true);
                    targetPlayer.LastResponded = DateTime.Now;
                }
            }
        }

        private async Task SendPacketsInQueueAsync()
        {
            while (true)
            {
                var (receiver, packet) = await Task.Run(() =>
                {
                    while (true)
                    {
                        if (_packets.TryDequeue(out var next))
                        {
                            return next;
                        }
                    }
                });

                var data = MessagePackSerializer.Serialize(packet);
                await _server.SendAsync(data, data.Length, receiver);
                Log.Debug($"Packet send: endpoint - {receiver}, id - {packet.Id}, type: {packet.Type}");
            }
        }

        private async Task DisconnectingTimeoutPlayersAsync()
        {
            while (true)
            {
                foreach (var (id, player) in _players)
                {
                    if (DateTime.Now - player.LastResponded < TimeSpan.FromSeconds(Timeout))
                    {
                        continue;
                    }

                    if (_players.TryRemove(id, out var removedPlayer))
                    {
                        var packet = new InfoPacket
                        {
                            Type = PacketType.Disconnect,
                            Id = removedPlayer.Id
                        };

                        SendPacketToAll(packet);
                        Log.Information($"Timeout player: endpoint - {removedPlayer.RemoteEndPoint}, id - {removedPlayer.Id}, name: {removedPlayer.Name}");
                    }
                }

                await Task.Delay(10);
            }
        }

        public void SendPacket(IPEndPoint ep, IPacket packet)
        {
            _packets.Enqueue((ep, packet));
        }

        public void SendPacketToAll(IPacket packet, bool isEcho = false, int excludeId = 0)
        {
            foreach (var (id, player) in _players)
            {
                if (id == excludeId)
                {
                    continue;
                }

                if (!isEcho)
                {
                    packet.Id = player.Id;
                    packet.Name = player.Name;
                }

                SendPacket(player.RemoteEndPoint, packet);
            }
        }

        private async Task<(IPacket packet, IPEndPoint sender)> ReceivePacketAsync()
        {
            var received = await _server.ReceiveAsync();
            var packet = MessagePackSerializer.Deserialize<IPacket>(received.Buffer);
            return (packet, received.RemoteEndPoint);
        }
    }
}
