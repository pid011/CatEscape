using UnityEngine;

namespace CatEscape.Network
{
    public class Player : MonoBehaviour
    {
        private int _hp;

        public int Id { get; set; }
        public string Name { get; set; }
        public int MaxHp { get; set; }
        public int Hp
        {
            get => _hp;
            set => _hp = Mathf.Clamp(value, 0, MaxHp);
        }
        public Vector2 Position
        {
            get => transform.position;
            set => transform.position = value;
        }
        public bool IsHost { get; set; }
        public bool IsMe { get; set; }
        public GamePacket.PlayerRole Role { get; set; }

        public void SyncData(GamePacket packet)
        {
            IsHost = packet.IsHost;
            MaxHp = packet.MaxHp;
            Hp = packet.Hp;
            var pos = new Vector2(packet.Position.x, packet.Position.y);
            Position = pos;
            Role = packet.Role;
        }

        public void SyncData(ref GamePacket packet)
        {
            packet.Id = Id;
            packet.Name = Name;
            packet.IsHost = IsHost;
            packet.MaxHp = MaxHp;
            packet.Hp = Hp;
            packet.Position = (Position.x, Position.y);
            packet.Role = Role;
        }
    }
}
