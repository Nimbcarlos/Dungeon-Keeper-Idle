using UnityEngine;


namespace DungeonKeeper
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private ResourceManager _resourceManager;
        [SerializeField] private TimeManager     _timeManager;
        [SerializeField] private SaveManager     _saveManager;

        public ResourceManager Resources => _resourceManager;
        public TimeManager     Time      => _timeManager;
        public SaveManager     Save      => _saveManager;

        public GameState CurrentState { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => SetState(GameState.Playing);

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            switch (newState)
            {
                case GameState.Playing:  _timeManager.Resume(); break;
                case GameState.Paused:   _timeManager.Pause();  break;
                case GameState.GameOver: _timeManager.Pause(); _saveManager.Save(); Debug.Log("Game Over"); break;
            }
        }
    }
}