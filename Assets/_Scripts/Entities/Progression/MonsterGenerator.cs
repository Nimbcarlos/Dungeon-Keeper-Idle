using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public static class MonsterGenerator
    {
        public static (MonsterData data, MonsterProgression progression, MonsterQuality quality, MonsterAffixData affixes) HatchEgg(EggData egg)
        {
            if (egg == null) return (null, null, MonsterQuality.Common, null);

            // 1. Sorteia a espécie
            MonsterData baseData = egg.GetRandomMonsterData();
            if (baseData == null)
            {
                Debug.LogError($"[MonsterGenerator] O ovo '{egg.eggName}' não possui nenhum MonsterData configurado!");
                return (null, null, MonsterQuality.Common, null);
            }

            // 2. Sorteia a qualidade
            MonsterQuality quality = egg.RollQuality();

            // 3. Sorteia as afixas e comportamentos nativos
            MonsterAffixData affixData = GenerateAffixesForQuality(quality);

            // 4. Cria a progressão do indivíduo
            MonsterProgression progression = new MonsterProgression
            {
                currentLevel = 1,
                currentXP = 0
            };

            Debug.Log($"🐣 [Hatchery] Nasceu um {baseData.displayName} ({quality})!");

            return (baseData, progression, quality, affixData);
        }

        /// <summary>
        /// Sorteia pares de habilidades a partir do pool global sem alterar o MonsterData.
        /// </summary>
        public static List<SkillNodeSO> GenerateSkillTreeFromPool(List<SkillNodeSO> globalPool, int maxLevel)
        {
            List<SkillNodeSO> generatedSkills = new List<SkillNodeSO>();
            if (globalPool == null || globalPool.Count < 2) return generatedSkills;

            List<SkillNodeSO> poolCopy = new List<SkillNodeSO>(globalPool);

            for (int i = 0; i < maxLevel; i++)
            {
                if (poolCopy.Count < 2) poolCopy = new List<SkillNodeSO>(globalPool); // Reseta pool se acabar

                int idx1 = UnityEngine.Random.Range(0, poolCopy.Count);
                SkillNodeSO left = poolCopy[idx1];
                poolCopy.RemoveAt(idx1);

                int idx2 = UnityEngine.Random.Range(0, poolCopy.Count);
                SkillNodeSO right = poolCopy[idx2];
                poolCopy.RemoveAt(idx2);

                generatedSkills.Add(left);
                generatedSkills.Add(right);
            }

            return generatedSkills;
        }

        // ── 🎲 LÓGICA DE AFIXAS E COMPORTAMENTOS (Sem alterações) ──

        public static MonsterAffixData GenerateAffixesForQuality(MonsterQuality quality)
        {
            MonsterAffixData affixData = new MonsterAffixData();

            switch (quality)
            {
                case MonsterQuality.Common:
                    affixData.modifiers.Add(GetRandomStatModifier());
                    break;

                case MonsterQuality.Uncommon:
                    affixData.modifiers.Add(GetRandomStatModifier());
                    affixData.modifiers.Add(GetRandomStatModifier());
                    break;

                case MonsterQuality.Rare:
                    for (int i = 0; i < 3; i++) 
                        affixData.modifiers.Add(GetRandomStatModifier());
                    break;

                case MonsterQuality.Epic:
                    for (int i = 0; i < 3; i++) 
                        affixData.modifiers.Add(GetRandomStatModifier());
                    affixData.behaviors.Add(GetRandomBehavior());
                    break;

                case MonsterQuality.Legendary:
                    for (int i = 0; i < 4; i++) 
                        affixData.modifiers.Add(GetRandomStatModifier());
                    affixData.behaviors.Add(GetRandomBehavior());
                    affixData.behaviors.Add(GetRandomBehavior());
                    break;
            }

            return affixData;
        }

        private static StatModifier GetRandomStatModifier()
        {
            Array statTypes = Enum.GetValues(typeof(SkillType));
            SkillType randomType = (SkillType)statTypes.GetValue(UnityEngine.Random.Range(0, statTypes.Length));

            float randomValue = UnityEngine.Random.Range(0.05f, 0.15f);

            return new StatModifier
            {
                type = randomType,
                value = randomValue
            };
        }

        private static BehaviorType GetRandomBehavior()
        {
            Array behaviors = Enum.GetValues(typeof(BehaviorType));
            return (BehaviorType)behaviors.GetValue(UnityEngine.Random.Range(1, behaviors.Length));
        }
    }
}