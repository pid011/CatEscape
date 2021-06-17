using System;

namespace CatEscape.Server
{
    public class PlayerEventArgs : EventArgs
    {
        public ServerPlayer Player { get; }

        public PlayerEventArgs(ServerPlayer player)
        {
            Player = player;
        }
    }
}
