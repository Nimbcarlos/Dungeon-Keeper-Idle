using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public enum EggRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(fileName = "NewEggData", menuName = "Dungeon/Egg Data")]
    public class EggData : ScriptableObject
    {
        [Header("Identificação")]
        public string eggID;
        public string eggName;
        public Sprite eggIcon;
        public GameObject eggPrefab; // Prefab 3D/Sprite para exibição animada

        [Header("Regras de Incubação")]
        public EggRarity rarity = EggRarity.Common;
        public float hatchTimeSeconds = 300f; // Ex: 5 minutos
        public int goldHatchCost = 50;

        [Header("Pool de Monstros Possíveis")]
        public List<MonsterData> possibleMonsters = new List<MonsterData>();

        [Header("Chances de Qualidade do Monstro Resultante (%)")]
        [Range(0, 100)] public float commonChance = 60f;
        [Range(0, 100)] public float uncommonChance = 25f;
        [Range(0, 100)] public float rareChance = 10f;
        [Range(0, 100)] public float epicChance = 4f;
        [Range(0, 100)] public float legendaryChance = 1f;

        [Header("Pesos de Atributos Preferenciais")]
        public StatWeight[] preferredStatWeights;

        /// <summary>
        /// Sorteia a qualidade do monstro gerado com base nas probabilidades do ovo.
        /// </summary>
        public MonsterQuality RollQuality()
        {
            float total = commonChance + uncommonChance + rareChance + epicChance + legendaryChance;
            float roll = Random.Range(0f, total);

            if ((roll -= legendaryChance) < 0) return MonsterQuality.Legendary;
            if ((roll -= epicChance) < 0) return MonsterQuality.Epic;
            if ((roll -= rareChance) < 0) return MonsterQuality.Rare;
            if ((roll -= uncommonChance) < 0) return MonsterQuality.Uncommon;
            return MonsterQuality.Common;
        }

        /// <summary>
        /// Seleciona um MonsterData aleatório do pool.
        /// </summary>
        public MonsterData GetRandomMonsterData()
        {
            if (possibleMonsters == null || possibleMonsters.Count == 0) return null;
            return possibleMonsters[Random.Range(0, possibleMonsters.Count)];
        }

        [System.Serializable]
        public struct StatWeight
        {
            public SkillType statType;
            public int weight; // Quanto maior o peso, maior a chance de ser sorteado

            // Método de sorteio baseado na qualidade e nos pesos disponíveis
            public static SkillType GetRandomStatByQuality(
                MonsterQuality quality,
                StatWeight[] availableStats)
            {
                if (availableStats == null || availableStats.Length == 0)
                    throw new System.ArgumentException("Nenhum atributo disponível.", nameof(availableStats));

                int totalWeight = 0;
                foreach (StatWeight stat in availableStats)
                    totalWeight += System.Math.Max(0, stat.weight);

                if (totalWeight == 0)
                    return availableStats[0].statType;

                int roll = UnityEngine.Random.Range(0, totalWeight);
                foreach (StatWeight stat in availableStats)
                {
                    roll -= System.Math.Max(0, stat.weight);
                    if (roll < 0)
                        return stat.statType;
                }

                return availableStats[availableStats.Length - 1].statType;
            }
        }
    }
}