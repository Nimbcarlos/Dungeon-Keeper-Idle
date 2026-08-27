using System.Collections.Generic;

namespace DungeonKeeper
{
    public enum BehaviorType
    {
        None,
        Piercing,
        Ricochet,
        Multishot,
        Splash,
        StatusPoison,
        StatusBurn,
        StatusFreeze,
        Thorns,
        Curse
    }

    [System.Serializable]
    public struct StatModifier
    {
        public SkillType type;
        public float value;
    }

    [System.Serializable]
    public class MonsterAffixData
    {
        public List<StatModifier> modifiers = new List<StatModifier>();
        public List<BehaviorType> behaviors = new List<BehaviorType>();
    }
}