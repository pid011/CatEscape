using System;
using CatEscape.Network;
using UnityEngine;

namespace CatEscape.Game
{
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _mapOffset;
        [SerializeField] private float _speed;
        private Player _player;

        private GamePacket _movePacket = new GamePacket {Type = PacketType.Move};

        private void Awake()
        {
            _player = GetComponent<Player>();
        }

        private void Update()
        {
            if (!_player.IsHost)
            {
                return;
            }

            var xAxis = Input.GetAxis("Horizontal");

            if (xAxis != 0)
            {
                var translatedPos = new Vector2(xAxis * _speed * Time.deltaTime, transform.position.y);
                translatedPos.x = Mathf.Clamp(translatedPos.x, -_mapOffset, _mapOffset);
                transform.position = translatedPos;
                
                _player.SyncData(ref _movePacket);
                NetworkManager.SendPacket(_movePacket);
            }
        }
    }
}
