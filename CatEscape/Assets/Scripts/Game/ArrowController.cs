using UnityEngine;

namespace CatEscape.Game
{
    public class ArrowController : MonoBehaviour
    {
        private GameDirector _director;
        private GameObject _player;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _director = GameObject.FindGameObjectWithTag("GameDirector").GetComponent<GameDirector>();
        }

        // Update is called once per frame
        private void Update()
        {
            //transform.Translate(0, -0.1f, 0);

            //if (transform.position.y < -5.0f)
            //{
            //    Destroy(gameObject);
            //}

            //var p1 = transform.position;
            //var p2 = _player.transform.position;
            //var dir = p1 - p2;
            //var d = dir.magnitude;

            //const float r1 = 0.5f;
            //const float r2 = 1.0f;

            //if (d < r1 + r2)
            //{
            //    _director.DecreaseHp();
            //    Destroy(gameObject);
            //}
        }
    }
}
