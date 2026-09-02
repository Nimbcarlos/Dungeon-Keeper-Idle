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

        [Header("Progressão de Nível (Dados)")]
        public int currentLevel = 1;
        public int currentXP = 0;
        public int maxLevel = 25; // Teto do GDD[cite: 1]

        public int SkillSlots => (int)rarity + 1;

        // Adicione estes campos dentro do MonsterData:
        [Header("Tipo de Ataque")]
        public AttackType attackType = AttackType.Melee;

        [Header("Configurações de Ataque (SOs)")]
        public ProjectileData projectileData; // Usado se attackType == Ranged[cite: 15]
        public MeleeSkillData meleeData;           // Usado se attackType == Melee

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

        // No MonsterData.cs:

        [Header("Pool Global de Nós para Sorteio")]
        public List<SkillNodeSO> globalSkillPool = new List<SkillNodeSO>();

        /// <summary>
        /// Garante que se a lista do monstro estiver vazia, ele receba pares sorteados automaticamente do pool.
        /// </summary>
        public void EnsureAvailableSkillsPopulated()
        {
            // Se não tiver nó nenhum definido e tiver um pool global, gera o catálogo inicial do monstro
            if ((availableSkills == null || availableSkills.Count == 0) && globalSkillPool != null && globalSkillPool.Count >= 2)
            {
                availableSkills = new List<SkillNodeSO>();

                // Gera pares de opções até cobrir a quantidade de níveis do teto do monstro
                int pairsToGenerate = maxLevel; 
                for (int i = 0; i < pairsToGenerate; i++)
                {
                    var (left, right) = SkillRewardGenerator.GetRandomRewardPair(globalSkillPool);
                    if (left != null && right != null)
                    {
                        availableSkills.Add(left);
                        availableSkills.Add(right);
                    }
                }
                Debug.Log($"🎲 [MonsterData] Pool de habilidades gerado proceduralmente para {displayName} ({availableSkills.Count} opções)!");
            }
        }

        public static class SkillRewardGenerator
        {
            /// <summary>
            /// Sorteia 2 opções únicas de um pool de nós para apresentar ao jogador no Level Up.
            /// </summary>
            public static (SkillNodeSO leftOption, SkillNodeSO rightOption) GetRandomRewardPair(List<SkillNodeSO> globalPool)
            {
                if (globalPool == null || globalPool.Count < 2)
                {
                    Debug.LogWarning("[SkillRewardGenerator] Pool insuficiente de SkillNodeSO para sortear o par!");
                    return (null, null);
                }

                // Cria uma cópia da lista para não alterar o pool original
                List<SkillNodeSO> availablePool = new List<SkillNodeSO>(globalPool);

                // Sorteia o primeiro nó
                int index1 = Random.Range(0, availablePool.Count);
                SkillNodeSO left = availablePool[index1];
                availablePool.RemoveAt(index1); // Remove para não repetir o mesmo no lado direito

                // Sorteia o segundo nó
                int index2 = Random.Range(0, availablePool.Count);
                SkillNodeSO right = availablePool[index2];

                return (left, right);
            }
        }

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

        public void ClaimReward(int level, SkillNodeSO chosenNode)
        {
            if (chosenNode == null) return;

            if (unlockedSkillIDs == null) unlockedSkillIDs = new List<string>();
            if (claimedRewards == null) claimedRewards = new List<ClaimedReward>();

            // 1. Registra o ID para consultas de exclusão e UI
            if (!unlockedSkillIDs.Contains(chosenNode.skillID))
            {
                unlockedSkillIDs.Add(chosenNode.skillID);
            }

            // 2. Salva o marco do nível como reivindicado
            ClaimedReward reward = claimedRewards.Find(r => r.levelUnlocked == level);
            if (reward == null)
            {
                claimedRewards.Add(new ClaimedReward
                {
                    levelUnlocked = level,
                    chosenOptionID = chosenNode.skillID
                });
            }
            else
            {
                reward.chosenOptionID = chosenNode.skillID;
            }
        }

    }
}
