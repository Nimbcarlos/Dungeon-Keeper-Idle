using UnityEngine;
using DungeonKeeper;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Dungeon/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Identificação")]
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

    [Header("Raridade")]
    public MonsterRarity rarity = MonsterRarity.Normal;

    [Header("Progressão de Nível (Dados)")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int maxLevel = 25; // Teto do GDD[cite: 1]


    public int SkillSlots => (int)rarity + 1;

    public int LevelCap => rarity switch
    {
        MonsterRarity.Normal   => 5,
        MonsterRarity.Uncommon => 10,
        MonsterRarity.Rare     => 15,
        MonsterRarity.Epic     => 20,
        MonsterRarity.Legendary => 25,
        _                      => 5
    };

    [Header("Stats Base")]
    public Stats stats;

    [Header("Crescimento por Nível")]
    public int   hpPerLevel     = 10;
    public int   attackPerLevel = 2;
    public float speedPerLevel  = 0f;

    [Header("XP")]
    public int   baseXPRequired = 100;
    public float xpGrowthRate   = 1.5f;

    [Header("Comportamento")]
    public MonsterBehavior defaultBehavior = MonsterBehavior.Defensive;

    [Header("Loot")]
    public int goldReward    = 5;
    public int essenceReward = 1;

    public int GetXPRequired(int level)
    {
        return Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpGrowthRate, level - 1));
    }

    public Stats GetStatsForLevel(int level)
    {
        Stats s = stats.Clone();
        int bonus = level - 1;
        s.maxHP       += hpPerLevel     * bonus;
        s.attackPower += attackPerLevel * bonus;
        s.moveSpeed   += speedPerLevel  * bonus;
        return s;
    }
}

public enum MonsterBehavior
{
    Defensive,
    Aggressive,
    Ranged,
    Support,
    Custom
}

public enum MonsterRarity
{
    Normal,
    Uncommon,
    Rare,
    Epic,
    Legendary
}