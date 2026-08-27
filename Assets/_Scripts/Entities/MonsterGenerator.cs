using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public static class MonsterGenerator
    {
        public static MonsterAffixData GenerateAffixesForQuality(MonsterQuality quality)
        {
            MonsterAffixData affixData = new MonsterAffixData();

            switch (quality)
            {
                case MonsterQuality.Common:
                    // 1 Modificador de Atributo
                    affixData.modifiers.Add(GetRandomStatModifier());
                    break;

                case MonsterQuality.Uncommon:
                    // 2 Modificadores de Atributo
                    affixData.modifiers.Add(GetRandomStatModifier());
                    affixData.modifiers.Add(GetRandomStatModifier());
                    break;

                case MonsterQuality.Rare:
                    // 3 Modificadores de Atributo
                    for (int i = 0; i < 3; i++) 
                        affixData.modifiers.Add(GetRandomStatModifier());
                    break;

                case MonsterQuality.Epic:
                    // 3 Modificadores + 1 Comportamento Garantido
                    for (int i = 0; i < 3; i++) 
                        affixData.modifiers.Add(GetRandomStatModifier());
                    affixData.behaviors.Add(GetRandomBehavior());
                    break;

                case MonsterQuality.Legendary:
                    // 4 Modificadores + 2 Comportamentos Garantidos
                    for (int i = 0; i < 4; i++) 
                        affixData.modifiers.Add(GetRandomStatModifier());
                    affixData.behaviors.Add(GetRandomBehavior());
                    affixData.behaviors.Add(GetRandomBehavior());
                    break;
            }

            return affixData;
        }

        // 🎲 Sorteia um modificador de status e um valor aleatório
        private static StatModifier GetRandomStatModifier()
        {
            // Pega todos os tipos de atributos disponíveis no enum SkillType
            Array statTypes = Enum.GetValues(typeof(SkillType));
            SkillType randomType = (SkillType)statTypes.GetValue(UnityEngine.Random.Range(0, statTypes.Length));

            // Sorteia um valor de bônus baseado no tipo (ex: entre 5% e 15%)
            float randomValue = UnityEngine.Random.Range(0.05f, 0.15f);

            return new StatModifier
            {
                type = randomType,
                value = randomValue
            };
        }

        // 🎲 Sorteia um comportamento mecânico único
        private static BehaviorType GetRandomBehavior()
        {
            // Pega os comportamentos excluindo o 'None' (índice 0)
            Array behaviors = Enum.GetValues(typeof(BehaviorType));
            return (BehaviorType)behaviors.GetValue(UnityEngine.Random.Range(1, behaviors.Length));
        }
    }
}