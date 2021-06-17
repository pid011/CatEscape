using System;
using System.Net;

namespace CatEscape.Server
{
    public class ServerPlayer
    {
        public IPEndPoint RemoteEndPoint { get; }
        public int Id { get; }
        public string Name { get; }
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public (float x, float y) Position { get; set; }
        public GamePacket.PlayerRole Role { get; set; }
        public bool IsHost { get; set; }
        public DateTime LastResponded { get; set; }

        public ServerPlayer(int id, string name, IPEndPoint ep)
        {
            Id = id;
            Name = name;
            RemoteEndPoint = ep;
            LastResponded = DateTime.Now;
        }

        public void Sync(GamePacket packet)
        {
            MaxHp = packet.MaxHp;
            Hp = packet.Hp;
            Position = packet.Position;
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
                Position = Position,
                Role = Role
            };
        }
    }
}
