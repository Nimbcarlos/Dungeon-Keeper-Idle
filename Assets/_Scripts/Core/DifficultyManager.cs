using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private Treasure _treasure;

    // thresholds de Gold → nível de ameaça
    [SerializeField] private int[] _goldThresholds = { 0, 50, 150, 300, 500 };

    public int CurrentDifficulty { get; private set; } = 1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (_treasure == null) return;

        int gold = _treasure.Gold;
        int newDifficulty = 1;

        for (int i = _goldThresholds.Length - 1; i >= 0; i--)
        {
            if (gold >= _goldThresholds[i])
            {
                newDifficulty = i + 1;
                break;
            }
        }

        if (newDifficulty != CurrentDifficulty)
        {
            CurrentDifficulty = newDifficulty;
            Debug.Log($"Dificuldade: {CurrentDifficulty} (Gold: {gold})");
        }
    }
}