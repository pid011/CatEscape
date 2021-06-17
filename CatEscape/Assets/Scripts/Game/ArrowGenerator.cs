using UnityEngine;

namespace CatEscape.Game
{
    public class ArrowGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject _arrowPrefab;

        private readonly float _spawn = 1.0f;
        private float _delta;

        private void Update()
        {
            _delta += Time.deltaTime;
            if (_delta > _spawn)
            {
                _delta = 0f;
                var go = Instantiate(_arrowPrefab);
                var posX = Random.Range(-6, 7);
                go.transform.position = new Vector3(posX, 7, 0);
            }
        }
    }
}
