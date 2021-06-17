using System;
using System.Collections.Generic;
using CatEscape.Network;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace CatEscape.Game
{
    public class GameDirector : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Image _hpGauge;

        private Player _me;
        private Dictionary<int, Player> _players;

        private void Awake()
        {
            _players = new Dictionary<int, Player>();
        }

        private void Start()
        {
            var packet = new InfoPacket
            {
                Type = PacketType.Ready,
                Id = NetworkManager.Id
            };
            NetworkManager.SendPacket(packet);
        }

        private void Update()
        {
            if (NetworkManager.TryGetNextPacket(out var packet))
            {
                HandlePacket(packet);
            }
        }

        private void HandlePacket(IPacket packet)
        {
            if (packet.Type == PacketType.PlayerJoin)
            {
                try
                {
                    var isMe = packet.Id == NetworkManager.Id;
                    var createdPlayer = CreatePlayerInstance(isMe, packet.IsHost, packet.Id, packet.Name);
                    _players.Add(packet.Id, createdPlayer);

                    if (createdPlayer.IsMe)
                    {
                        _me = createdPlayer;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                return;
            }

            if (!_players.TryGetValue(packet.Id, out var targetPlayer))
            {
                return;
            }

            if (packet is GamePacket gamePacket)
            {
                targetPlayer.SyncData(gamePacket);

                // todo
            }
        }

        private Player CreatePlayerInstance(bool isMe, bool isHost, int playerId, string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentNullException(nameof(playerName));
            }

            if (isMe && !(_me is null))
            {
                throw new ArgumentException("본인 플레이어가 이미 존재합니다.");
            }

            if (_players.TryGetValue(playerId, out var foundPlayer))
            {
                throw new ArgumentException(
                    $"해당 플레이어가 이미 존재합니다: id - {foundPlayer.Id}, name - {foundPlayer.Name}");
            }

            var player = Instantiate(_playerPrefab).GetComponent<Player>();
            player.IsMe = isMe;
            player.IsHost = isHost;
            player.Id = playerId;
            player.Name = playerName;

            return player;
        }

        public void DecreaseHp()
        {
            _hpGauge.fillAmount -= 0.1f;
        }
    }
}
