using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CatEscape.Util;
using MessagePack;
using Debug = CatEscape.Util.Debug;

namespace CatEscape.Network
{
    public sealed class NetworkManager : Singleton<NetworkManagerComponent>
    {
        public static bool IsConnected => Instance.IsConnected;
        public static int Id => Instance.Id;
        public static string Name => Instance.Name;
        public static IPEndPoint ServerEndPoint => Instance.ServerEndPoint;

        public static async Task ConnectToServerAsync(string address, int port, string name)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(nameof(address));
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (IsConnected)
            {
                return;
            }

            var serverEp = new IPEndPoint(IPAddress.Parse(address), port);
            var newName = name;
            var newId = name.GetHashCode();

            var client = new UdpClient();
            try
            {
                var packetBuffer = MessagePackSerializer.Serialize<IPacket>(new GamePacket
                {
                    Type = PacketType.Connect,
                    Id = newId,
                    Name = newName
                });
                await client.SendAsync(packetBuffer, packetBuffer.Length, serverEp);

                Debug.Log($"Client LocalEndPoint: {client.Client.LocalEndPoint}");
                Debug.Log($"Server EndPoint: {serverEp}");

                var replyTask = client.ReceiveAsync();
                await Task.WhenAny(replyTask, Task.Delay(TimeSpan.FromSeconds(5)));
                if (!replyTask.IsCompleted)
                {
                    throw new TimeoutException("서버로부터 응답을 받지 못함");
                }

                var receivedPacket = MessagePackSerializer.Deserialize<IPacket>(replyTask.Result.Buffer);
                if (!(receivedPacket is ReplyPacket replyPacket))
                {
                    Debug.LogError("Reply가 아닌 다른 패킷을 받음");
                    throw new ConnectionFailException(ReplyPacket.Reasons.None);
                }

                if (!replyPacket.Result)
                {
                    throw new ConnectionFailException(replyPacket.Reason);
                }
                client.Connect(serverEp);

                if (!Instance.Init(client, newId, newName))
                {
                    throw new Exception("NetworkManager 초기화에 실패했습니다.");
                }
                Instance.StartConnection();
            }
            finally
            {
                if (!IsConnected)
                {
                    client.Close();
                }
            }
        }

        public static void SendPacket(IPacket packet)
        {
            Instance.SendPacket(packet);
        }

        public static bool TryGetNextPacket(out IPacket packet)
        {
            return Instance.TryGetNextPacket(out packet);
        }
    }
}
