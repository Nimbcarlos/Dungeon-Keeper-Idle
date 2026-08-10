using UnityEngine;


namespace DungeonKeeper
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float    _fps        = 12f;
        [SerializeField] private bool     _loop       = false;

        private SpriteRenderer _sr;
        private int   _currentFrame;
        private float _timer;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_frames.Length > 0)
                _sr.sprite = _frames[0];
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < 1f / _fps) return;

            _timer = 0f;
            _currentFrame++;

            if (_currentFrame >= _frames.Length)
            {
                if (_loop)
                    _currentFrame = 0;
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            _sr.sprite = _frames[_currentFrame];
        }
    }
}