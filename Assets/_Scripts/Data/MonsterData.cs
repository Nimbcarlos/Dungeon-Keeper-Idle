using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public enum RewardType
    {
        ModifierChoice,
        SkillUnlock,
        SkillUpgrade,
        PassiveChoice
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

    public enum AttackType
    {
        Melee,
        Ranged
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

        [Header("Tipo de Ataque")]
        public AttackType attackType = AttackType.Melee;

        [Header("Configurações de Ataque (SOs)")]
        public ProjectileData projectileData;
        public MeleeSkillData meleeData;

        [Header("Limites e Regras de Nível")]
        public int maxLevel = 25;
        public int baseXPRequired = 100;
        public float xpGrowthRate = 1.5f;

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
        public int hpPerLevel = 10;
        public int attackPerLevel = 2;
        public float speedPerLevel = 0f;

        [Header("Comportamento")]
        public MonsterBehavior defaultBehavior = MonsterBehavior.Defensive;

        [Header("Loot")]
        public int goldReward = 5;
        public int essenceReward = 1;

        [Header("Árvore de Habilidades (Catálogo Estático)")]
        public List<SkillNodeSO> availableSkills = new List<SkillNodeSO>();
        public List<SkillNodeSO> globalSkillPool = new List<SkillNodeSO>();

        public int GetXPRequired(int level)
        {
            return Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpGrowthRate, level - 1));
        }

        public Stats GetStatsForLevel(int level)
        {
            Stats s = stats.Clone();
            int bonus = level - 1;
            s.maxHP += hpPerLevel * bonus;
            s.attackPower += attackPerLevel * bonus;
            s.moveSpeed += speedPerLevel * bonus;
            return s;
        }
    }
}