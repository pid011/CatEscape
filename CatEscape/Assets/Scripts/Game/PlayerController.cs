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

        private void Awake()
        {
            _player = GetComponent<Player>();
        }

        private void Update()
        {
            if (!_player.IsMe)
            {
                return;
            }

            var xAxis = 0f;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                xAxis -= 1f;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                xAxis += 1f;
            }

            if (xAxis != 0)
            {
                var translatedPos = new Vector2(transform.position.x + (xAxis * _speed * Time.deltaTime), transform.position.y);
                translatedPos.x = Mathf.Clamp(translatedPos.x, -_mapOffset, _mapOffset);

                _player.Position = translatedPos;
                NetworkManager.SendPacket(_player.CreatePacket(PacketType.Move));
            }
        }
    }
}
