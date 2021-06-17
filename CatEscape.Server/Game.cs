using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CatEscape.Server
{
    public class Game
    {
        private readonly GameServer _server;
        private readonly ConcurrentDictionary<int, ServerPlayer> _joinedPlayers = new();
        private readonly CancellationTokenSource _cancellation = new();

        public Game(GameServer server)
        {
            _server = server;
            _server.PlayerJoin += OnPlayerJoin;
            _server.PlayerDisconnect += OnPlayerDisconnect;
        }

        public async Task RunAsync()
        {
            while (_server.Players.Count < GameServer.MaxPlayers)
            {
                await Task.Delay(10);
            }
        }

        private void OnPlayerJoin(object sender, PlayerEventArgs e)
        {
            _joinedPlayers.TryAdd(e.Player.Id, e.Player);
        }

        private void OnPlayerDisconnect(object sender, PlayerEventArgs e)
        {
            if (_joinedPlayers.TryRemove(e.Player.Id, out var _))
            {
                _cancellation.Cancel();
            }
        }
    }
}
