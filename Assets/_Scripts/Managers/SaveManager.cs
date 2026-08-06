using UnityEngine;

[System.Serializable] public class SaveData     { public PlayerData player; public DungeonData dungeon; public ProgressData progress; }
[System.Serializable] public class PlayerData   { public int gold; public int essence; }
[System.Serializable] public class DungeonData  { public MonsterSaveData[] monsters; }
[System.Serializable] public class MonsterSaveData { public string id; public int currentHP; public int xp; public int level; public int kills; public int deaths; }
[System.Serializable] public class ProgressData { public int difficulty; public float runTime; public string[] unlockedMonsters; }

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save() => Debug.Log("Save chamado");
    public void Load() => Debug.Log("Load chamado");
}