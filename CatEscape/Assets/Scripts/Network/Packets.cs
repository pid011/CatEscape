using MessagePack;

namespace CatEscape.Network
{
    public enum PacketType
    {
        /// <summary>
        /// 플레이어가 서버에 입장했음을 알림
        /// </summary>
        Connect,

        /// <summary>
        /// 서버에서 플레이어에게 게임이 시작됨을 알림
        /// </summary>
        GameStart,

        /// <summary>
        /// 플레이어 입장 알림
        /// </summary>
        /// 
        PlayerJoin,

        /// <summary>
        /// 서버에서 플레이에게 게임 시작 전 신호
        /// </summary>
        Ready,

        /// <summary>
        /// 서버에서 클라이언트에게
        /// </summary>
        TeamSelected,

        /// <summary>
        /// 게임 시작 전 카운트다운 (초 단위로 서버에서 플레이어에게 전송)
        /// </summary>
        CountDownBeforeStart,

        /// <summary>
        /// 게임 타이머 시간
        /// </summary>
        Timer,

        /// <summary>
        /// 이 신호를 받으면 게임 시작
        /// </summary>
        BeginPlay,

        /// <summary>
        /// 플레이어의 이동 신호 (위치도 함께 전송되어야 함)
        /// </summary>
        Move,

        /// <summary>
        /// 공격 플레이어가 공격키를 눌렀을 때 신호 (공격수가 송신, 수비수가 수신)
        /// </summary>
        Fire,

        /// <summary>
        /// 수비 플레이어가 데미지를 입었을 때 신호 (수비수가 송신, 공격수가 송신)
        /// </summary>
        Damaged,

        /// <summary>
        /// 게임을 이겼을 때 신호
        /// </summary>
        GameWin,

        /// <summary>
        /// 게임을 졌을 때 신호
        /// </summary>
        GameLoose,

        /// <summary>
        /// 플레이어와의 접속이 끊겼을 때 신호 (플레이어가 직접 신호를 송신하는 경우, Timeout되는 경우)
        /// </summary>
        Disconnect,

        /// <summary>
        /// 플레이어와의 연결이 살아있는지 확인하는 신호 (플레이어가 1초마다 송신, 서버에서 수신하여 respond타임 최신화)
        /// </summary>
        CheckSignal,

        /// <summary>
        /// 수신 측에서 송신 결과여부가 필요할 때 사용
        /// </summary>
        Reply,
    }

    [Union(0, typeof(InfoPacket))]
    [Union(1, typeof(GamePacket))]
    [Union(2, typeof(ReplyPacket))]
    [Union(3, typeof(CountdownPacket))]
    [Union(4, typeof(TimerPacket))]
    public interface IPacket
    {
        public PacketType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; }
    }

    [MessagePackObject]
    public class InfoPacket : IPacket
    {
        [Key(0)] public PacketType Type { get; set; }
        [Key(1)] public int Id { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public bool IsHost { get; set; }
    }

    [MessagePackObject]
    public class GamePacket : IPacket
    {
        public enum PlayerRole
        {
            Attacker,
            Defender
        }

        [Key(0)] public PacketType Type { get; set; }
        [Key(1)] public int Id { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public bool IsHost { get; set; }
        [Key(4)] public int MaxHp { get; set; }
        [Key(5)] public int Hp { get; set; }
        [Key(6)] public (float x, float y) Position { get; set; }
        [Key(7)] public PlayerRole Role { get; set; }
    }

    [MessagePackObject]
    public class ReplyPacket : IPacket
    {
        public enum Reasons
        {
            None,
            /// <summary>
            /// 서버에 같은 이름의 플레이어가 이미 접속해 있음
            /// </summary>
            NameOfPlayerIsAlreadyConnected,
            ServerIsFull
        }

        [Key(0)] public PacketType Type { get; set; }
        [Key(1)] public int Id { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public bool IsHost { get; set; }
        [Key(4)] public bool Result { get; set; }
        [Key(5)] public Reasons Reason { get; set; }

        public ReplyPacket()
        {
            Type = PacketType.Reply;
        }
    }

    [MessagePackObject]
    public class CountdownPacket : IPacket
    {
        [Key(0)] public PacketType Type { get; set; }
        [Key(1)] public int Id { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public bool IsHost { get; set; }
        [Key(4)] public int Countdown { get; set; }

        public CountdownPacket()
        {
            Type = PacketType.CountDownBeforeStart;
        }
    }

    [MessagePackObject]
    public class TimerPacket : IPacket
    {
        [Key(0)] public PacketType Type { get; set; }
        [Key(1)] public int Id { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(4)] public bool IsHost { get; set; }
        [Key(5)] public int Time { get; set; }

        public TimerPacket()
        {
            Type = PacketType.Timer;
        }
    }
}
