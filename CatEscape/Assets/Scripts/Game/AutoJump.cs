using UnityEngine;

namespace CatEscape.Game
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class AutoJump : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float _jumpForce;

        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }
}
