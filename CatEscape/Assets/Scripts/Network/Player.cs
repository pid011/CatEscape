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
        public Vector2 Position { get => transform.position; set => transform.position = value; }
        public bool IsHost { get; set; }
        public bool IsMe { get; set; }
        public GamePacket.PlayerRole Role { get; set; }

        public void Sync(GamePacket packet)
        {
            IsHost = packet.IsHost;
            MaxHp = packet.MaxHp;
            Hp = packet.Hp;
            Position = new Vector2(packet.Position.x, packet.Position.y);
            Role = packet.Role;
        }

        public GamePacket CreatePacket(PacketType type)
        {
            return new GamePacket
            {
                Type = type,
                Id = Id,
                Name = Name,
                IsHost = IsHost,
                MaxHp = MaxHp,
                Hp = Hp,
                Position = (Position.x, Position.y),
                Role = Role
            };
        }
    }
}
