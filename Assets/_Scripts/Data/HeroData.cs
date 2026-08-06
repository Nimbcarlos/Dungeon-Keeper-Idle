using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Dungeon/Hero Data")]
public class HeroData : ScriptableObject
{
    [Header("Identificação")]
    public string id;
    public string displayName;
    public Sprite portrait;
    public GameObject prefab;

    [Header("Stats")]
    public Stats stats;

    [Header("Party")]
    public int partyCost     = 2;
    public int minDifficulty = 1;

    [Header("Loot ao morrer")]
    public int goldReward    = 10;
    public int essenceReward = 2;
}