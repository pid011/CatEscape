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
        public static bool IsConnected { get; private set; }
        public static int Id { get; private set; }
        public static string Name { get; private set; }
        public static IPEndPoint ServerEndPoint { get; private set; }

        /// <summary>
        /// 서버에 연결을 시도합니다.
        /// </summary>
        /// <param name="address">서버 주소</param>
        /// <param name="port">서버 포트</param>
        /// <param name="name">게임에서 사용할 이름</param>
        /// <returns></returns>
        /// <exception cref="MessagePackSerializationException"></exception>
        /// <exception cref="TimeoutException"/>
        /// <exception cref="SocketException"/>
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

            var packet = new InfoPacket
            {
                Type = PacketType.Connect,
                Id = name.GetHashCode(),
                Name = name
            };

            var client = new UdpClient();
            try
            {
                var packetBuffer = MessagePackSerializer.Serialize((IPacket)packet);
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
                    IsConnected = false;
                    throw new ConnectionFailException(replyPacket.Reason);
                }
                client.Connect(serverEp);

                Id = packet.Id;
                Name = packet.Name;
                ServerEndPoint = serverEp;
                Instance.Client = client;

                IsConnected = true;
            }
            finally
            {
                if (Instance.Client is null)
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
