using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using UnityEngine;

namespace CatEscape.Network
{
    public sealed class NetworkManagerComponent : MonoBehaviour
    {
        private readonly ConcurrentQueue<IPacket> _packetsToSend = new ConcurrentQueue<IPacket>();
        private readonly ConcurrentQueue<IPacket> _packetsToReceived = new ConcurrentQueue<IPacket>();
        private readonly object _socketLock = new object();

        private CancellationTokenSource _cancellation;

        public UdpClient Client { get; set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _cancellation = new CancellationTokenSource();
            Task.Run(ListeningPackets, _cancellation.Token);
            Task.Run(SendingPackets, _cancellation.Token);
            Task.Run(SendingSignalAsync, _cancellation.Token);
        }

        private void Update()
        {
            if (_packetsToReceived.TryPeek(out var packet))
            {
                Debug.Log($"received: {packet.Type}");
            }
        }

        private async void OnApplicationQuit()
        {
            _cancellation.Cancel();
            if (!NetworkManager.IsConnected)
            {
                return;
            }

            var packet = new InfoPacket
            {
                Type = PacketType.Disconnect,
                Id = NetworkManager.Id
            };

            SendPacket(packet);
            while (_packetsToSend.Count != 0)
            {
                await Task.Delay(10);
            }
        }

        public void SendPacket(IPacket packet)
        {
            _packetsToSend.Enqueue(packet);
            Debug.Log($"{Client.Client.LocalEndPoint} packets to send   : {_packetsToSend.Count}");
        }

        public bool TryGetNextPacket(out IPacket packet)
        {
            Debug.Log($"{Client.Client.LocalEndPoint} packets to receive: {_packetsToReceived.Count}");
            return _packetsToReceived.TryDequeue(out packet);
        }

        private void ListeningPackets()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                byte[] buffer;
                var endpoint = new IPEndPoint(IPAddress.None, 0);
                lock (_socketLock)
                {
                    buffer = Client.Receive(ref endpoint);
                }
                var packet = MessagePackSerializer.Deserialize<IPacket>(buffer);
                _packetsToReceived.Enqueue(packet);
            }
        }

        private void SendingPackets()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                if (NetworkManager.ServerEndPoint is null || !_packetsToSend.TryDequeue(out var next))
                {
                    continue;
                }

                var data = MessagePackSerializer.Serialize(next);
                lock (_socketLock)
                {
                    Client.Send(data, data.Length);
                }
            }
        }

        private async Task SendingSignalAsync()
        {
            var signal = new InfoPacket
            {
                Type = PacketType.CheckSignal
            };

            while (!_cancellation.IsCancellationRequested)
            {
                if (NetworkManager.IsConnected && !(NetworkManager.ServerEndPoint is null))
                {
                    signal.Id = NetworkManager.Id;
                    SendPacket(signal);
                }
                await Task.Delay(1000);
            }
        }
    }
}
