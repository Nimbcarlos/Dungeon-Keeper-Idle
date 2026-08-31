using UnityEngine;
using System.Collections.Generic;
using DungeonKeeper;

namespace DungeonKeeper
{
    public enum RewardType
    {
        ModifierChoice, // Escolha par-a-par de Status Base (ex: +10% Crit vs +5% Life Leech)
        SkillUnlock,    // Desbloqueia uma nova habilidade ativa (ex: Lançar Bola de Fogo)
        SkillUpgrade,   // Aumenta o nível de uma skill existente (ex: Burn no hit +2s)
        PassiveChoice   // Escolha de passiva única do monstro
    }

    [System.Serializable]
    public class ClaimedReward
    {
        public int levelUnlocked;
        public string chosenOptionID;
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
            MonsterRarity.Normal    => 10,
            MonsterRarity.Uncommon  => 20,
            MonsterRarity.Rare      => 30,
            MonsterRarity.Epic      => 40,
            MonsterRarity.Legendary => 50,
            _                       => 10
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

        [Header("Árvore de Habilidades (Dados)")]
        public List<SkillNodeSO> availableSkills = new List<SkillNodeSO>();
        public List<string> unlockedSkillIDs = new List<string>();

        [Header("Registro de Recompensas de Nível")]
        public List<ClaimedReward> claimedRewards = new List<ClaimedReward>();

        // ── MÉTODOS DE CONSULTA DA SKILL TREE & RECOMPENSAS ──

        /// <summary>
        /// Consulta se a recompensa do marco de um nível específico já foi reinvindicada.
        /// </summary>
        public bool IsLevelRewardClaimed(int level)
        {
            return claimedRewards != null && claimedRewards.Exists(r => r.levelUnlocked == level);
        }

        /// <summary>
        /// Retorna a lista de níveis alcançados que ainda possuem escolhas pendentes.
        /// </summary>
        public List<int> GetPendingRewardLevels()
        {
            List<int> pendingLevels = new List<int>();

            for (int lvl = 2; lvl <= currentLevel; lvl++)
            {
                if (!IsLevelRewardClaimed(lvl))
                {
                    pendingLevels.Add(lvl);
                }
            }

            return pendingLevels;
        }

        /// <summary>
        /// Retorna se uma habilidade específica da árvore já foi comprada/desbloqueada.
        /// </summary>
        public bool IsSkillUnlocked(string skillID)
        {
            return unlockedSkillIDs != null && unlockedSkillIDs.Contains(skillID);
        }

        /// <summary>
        /// Calcula o saldo real de pontos de skill disponíveis para gastar.
        /// </summary>
        public int GetAvailablePoints()
        {
            int totalEarned = Mathf.Max(0, currentLevel - 1);
            int spent = 0;

            if (unlockedSkillIDs != null)
            {
                foreach (string id in unlockedSkillIDs)
                {
                    SkillNodeSO node = availableSkills.Find(n => n != null && n.skillID == id);
                    if (node != null) spent += node.skillPointCost;
                }
            }

            return Mathf.Max(0, totalEarned - spent);
        }

        // ── MÉTODOS BASE DE CÁLCULO E STATS ──

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
}
/*
using UnityEngine;
using System.Collections.Generic;
using DungeonKeeper;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Dungeon/Monster Data")]
public class MonsterData : ScriptableObject
{
    [System.Serializable]
        public class ClaimedReward
        {
            public int levelUnlocked;
            public string chosenOptionID;
        }

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
        MonsterRarity.Normal   => 10,
        MonsterRarity.Uncommon => 20,
        MonsterRarity.Rare     => 30,
        MonsterRarity.Epic     => 40,
        MonsterRarity.Legendary => 50,
        _                      => 10
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

    [Header("Árvore de Habilidades (Dados)")]
    public List<SkillNodeSO> availableSkills = new List<SkillNodeSO>();
    public List<string> unlockedSkillIDs = new List<string>();

    [Header("Registro de Recompensas de Nível")]

    public List<ClaimedReward> claimedRewards = new List<ClaimedReward>();


    // Consulta se o marco daquele nível já foi reivindicado
    public bool IsLevelRewardClaimed(int level)
    {
        return claimedRewards != null && claimedRewards.Exists(r => r.levelUnlocked == level);
    }

    // Retorna se o nó já foi comprado
    public bool IsSkillUnlocked(string skillID)
    {
        return unlockedSkillIDs != null && unlockedSkillIDs.Contains(skillID);
    }

    public int GetAvailablePoints(MonsterData data)
    {
        if (data == null) return 0;

        int totalEarned = Mathf.Max(0, data.currentLevel - 1);
        int spent = 0;

        if (data.unlockedSkillIDs != null)
        {
            foreach (string id in data.unlockedSkillIDs)
            {
                SkillNodeSO node = data.availableSkills.Find(n => n != null && n.skillID == id);
                if (node != null) spent += node.skillPointCost;
            }
        }

        return Mathf.Max(0, totalEarned - spent);
    }

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
*/