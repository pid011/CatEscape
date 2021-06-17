using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CatEscape.Network;
using MessagePack;
using UnityEngine;

namespace CatEscape
{
    public class NetworkManagerComponent : MonoBehaviour
    {
        private readonly ConcurrentQueue<IPacket> _packetsToSend = new ConcurrentQueue<IPacket>();
        private readonly ConcurrentQueue<IPacket> _packetsToReceived = new ConcurrentQueue<IPacket>();

        private UdpClient _client;

        public bool IsConnected { get; private set; }

        private Thread _listeningPacketsThread;
        private Thread _sendingPacketsThread;
        private Thread _sendingSignalsThread;

        public int Id { get; private set; }
        public string Name { get; private set; }
        public IPEndPoint ServerEndPoint { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            if (!IsConnected)
            {
                return;
            }

            _sendingSignalsThread.Abort();
            SendPacket(new GamePacket { Type = PacketType.Disconnect, Id = Id });

            // 모든 패킷이 보내질 때까지 기다림
            var waitTask = Task.Run(() => { while (_packetsToSend.Count != 0) { } });
            waitTask.Wait();

            _listeningPacketsThread.Abort();
            _sendingPacketsThread.Abort();
        }

        public bool Init(UdpClient client, int id, string name)
        {
            if (client.Client.RemoteEndPoint is null)
            {
                return false;
            }

            _client = client;
            ServerEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            Id = id;
            Name = name;

            return true;
        }

        public void StartConnection()
        {
            if (_client is null)
            {
                throw new NullReferenceException("UdpClient가 null입니다.");
            }

            if (IsConnected)
            {
                return;
            }

            IsConnected = true;
            _listeningPacketsThread = new Thread(ListeningPackets) { IsBackground = true };
            _listeningPacketsThread.Start();

            _sendingPacketsThread = new Thread(SendingPackets) { IsBackground = true };
            _sendingPacketsThread.Start();

            _sendingSignalsThread = new Thread(SendingSignals) { IsBackground = true };
            _sendingSignalsThread.Start();
        }

        private void ListeningPackets()
        {
            while (true)
            {
                try
                {
                    var clientEp = new IPEndPoint(IPAddress.Any, 0);
                    var buffer = _client.Receive(ref clientEp);
                    _packetsToReceived.Enqueue(MessagePackSerializer.Deserialize<IPacket>(buffer));
                }
                catch (SocketException e)
                {
                    Debug.LogError($"eror code: {e.SocketErrorCode}");
                }
            }
        }

        private void SendingPackets()
        {
            while (true)
            {
                if (!_packetsToSend.TryDequeue(out var next))
                {
                    continue;
                }
                var buffer = MessagePackSerializer.Serialize(next);
                _client.Send(buffer, buffer.Length);
            }
        }

        private void SendingSignals()
        {
            var signal = new GamePacket
            {
                Type = PacketType.CheckSignal,
                Id = NetworkManager.Id
            };

            while (true)
            {
                SendPacket(signal);
                Thread.Sleep(1000);
            }
        }

        public void SendPacket(IPacket packet)
        {
            _packetsToSend.Enqueue(packet);
        }

        public bool TryGetNextPacket(out IPacket packet)
        {
            return _packetsToReceived.TryDequeue(out packet);
        }
    }
}
