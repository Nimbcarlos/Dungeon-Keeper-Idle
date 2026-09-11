using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public enum SummoningPresentation { Egg, Crystal, Relic, Scroll, Other }

    [CreateAssetMenu(fileName = "SummoningItem", menuName = "Dungeon/Items/Summoning Item")]
    public class SummoningItemData : ItemData
    {
        [Header("Invocacao")]
        public SummoningPresentation presentation = SummoningPresentation.Egg;
        [Tooltip("Duracao em segundos reais. O controller cuidara do horario externo e do save.")]
        [Min(0)] public int summonDurationSeconds = 300;

        [Header("Especies Possiveis — Pesos Relativos")]
        public List<MonsterOutcome> possibleMonsters = new();

        [Header("Qualidade do Monstro — Pesos Relativos")]
        public List<QualityOutcome> qualityWeights = new()
        {
            new QualityOutcome { quality = MonsterQuality.Common, weight = 60 },
            new QualityOutcome { quality = MonsterQuality.Uncommon, weight = 25 },
            new QualityOutcome { quality = MonsterQuality.Rare, weight = 10 },
            new QualityOutcome { quality = MonsterQuality.Epic, weight = 4 },
            new QualityOutcome { quality = MonsterQuality.Legendary, weight = 1 }
        };

        [Header("Afixos Preferenciais — Configuracao Para o Gerador")]
        [Tooltip("Lista vazia indica usar a regra padrao do gerador. A integracao sera feita no gerador de invocacoes.")]
        public List<AffixWeight> preferredAffixes = new();

        [Header("Aceleracao por Anuncio — Opcional")]
        public bool allowRewardedAcceleration = false;
        [Min(0)] public int secondsReducedPerAd = 60;
        [Min(0)] public int maxAdsPerSummon = 1;

        [Serializable]
        public class MonsterOutcome
        {
            public MonsterData monster;
            [Min(0)] public int weight = 1;
        }

        [Serializable]
        public class QualityOutcome
        {
            public MonsterQuality quality;
            [Min(0)] public int weight = 1;
        }

        [Serializable]
        public class AffixWeight
        {
            public SkillType attribute;
            [Min(0)] public int weight = 1;
        }

        public bool TryValidate(out string error)
        {
            if (string.IsNullOrWhiteSpace(itemID)) return Fail("Preencha Item ID.", out error);
            if (string.IsNullOrWhiteSpace(displayName)) return Fail("Preencha Display Name.", out error);
            if (essencePrice < 0 || quantityPerPurchase < 1 || summonDurationSeconds < 0)
                return Fail("Preco, quantidade ou duracao invalidos.", out error);

            long monsterWeight = 0;
            var species = new HashSet<MonsterData>();
            if (possibleMonsters != null)
                foreach (var entry in possibleMonsters)
                {
                    if (entry == null || entry.monster == null || entry.weight < 0)
                        return Fail("Entrada de especie invalida.", out error);
                    if (!species.Add(entry.monster)) return Fail("Especie repetida no pool.", out error);
                    if (string.IsNullOrWhiteSpace(entry.monster.id) || entry.monster.prefab == null)
                        return Fail("Especie sem ID ou prefab.", out error);
                    monsterWeight += entry.weight;
                }
            if (monsterWeight <= 0) return Fail("Adicione uma especie com peso positivo.", out error);

            long qualityWeight = 0;
            var qualities = new HashSet<MonsterQuality>();
            if (qualityWeights != null)
                foreach (var entry in qualityWeights)
                {
                    if (entry == null || entry.weight < 0 || !Enum.IsDefined(typeof(MonsterQuality), entry.quality))
                        return Fail("Entrada de qualidade invalida.", out error);
                    if (!qualities.Add(entry.quality)) return Fail("Qualidade repetida.", out error);
                    qualityWeight += entry.weight;
                }
            if (qualityWeight <= 0) return Fail("Adicione uma qualidade com peso positivo.", out error);

            long affixWeight = 0;
            var attributes = new HashSet<SkillType>();
            if (preferredAffixes != null)
                foreach (var entry in preferredAffixes)
                {
                    if (entry == null || entry.weight < 0 || !Enum.IsDefined(typeof(SkillType), entry.attribute))
                        return Fail("Entrada de afixo invalida.", out error);
                    if (!attributes.Add(entry.attribute)) return Fail("Atributo repetido.", out error);
                    affixWeight += entry.weight;
                }
            if (preferredAffixes != null && preferredAffixes.Count > 0 && affixWeight <= 0)
                return Fail("Afixos configurados precisam de peso positivo.", out error);

            if (allowRewardedAcceleration && (secondsReducedPerAd <= 0 || maxAdsPerSummon <= 0))
                return Fail("Configure a reducao e o limite de anuncios.", out error);

            error = null;
            return true;
        }

        private static bool Fail(string message, out string error)
        {
            error = message;
            return false;
        }

        [ContextMenu("Validate Item")]
        private void ValidateItem()
        {
            if (TryValidate(out string error)) Debug.Log("Item valido. Confira tambem a unicidade do ID no catalogo.", this);
            else Debug.LogError(error, this);
        }
    }
}
