using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    [System.Serializable]
    public class TalentOption
    {
        public string optionID;
        public string sourceSkillID;
        public int requiredLevel;
    }

    [System.Serializable]
    public class ClaimedReward
    {
        public int levelUnlocked;
        public string chosenOptionID;
    }

    [System.Serializable]
    public class MonsterProgression
    {
        public int currentLevel = 1;
        public int currentXP = 0;
        public bool birthAttackRolled;
        public string birthAttackID;
        public AttackType birthAttackType;
        public int combatTreeVersion;

        public AttackSpecialization GetBirthAttack(MonsterData data)
        {
            return data?.birthAttacks?.Find(a => a != null && a.id == birthAttackID);
        }

        public void EnsureBirthAttack(MonsterData data)
        {
            if (birthAttackRolled || data == null) return;
            bool legacy = talentsGenerated || (talentOptions?.Count ?? 0) > 0 ||
                (availableSkillIDs?.Count ?? 0) > 0 || (unlockedSkillIDs?.Count ?? 0) > 0;
            var options = data.birthAttacks?.FindAll(a => a != null && a.IsConfigured && a.rollWeight > 0);
            AttackSpecialization chosen = null;
            // Existing individuals keep their previous combat type.
            if (legacy) chosen = options?.Find(a => a.attackType == data.attackType);
            else if (options != null && options.Count > 0)
            {
                double total = 0;
                foreach (var option in options) total += option.rollWeight;
                double roll = Random.value * total;
                foreach (var option in options)
                {
                    chosen = option;
                    roll -= option.rollWeight;
                    if (roll < 0) break;
                }
            }
            birthAttackID = chosen != null ? chosen.id : "";
            birthAttackType = chosen != null ? chosen.attackType : data.attackType;
            birthAttackRolled = true;
        }

        public bool SupportsNode(SkillNodeSO node)
        {
            return node != null && (node.attackCompatibility == AttackCompatibility.Any ||
                (node.attackCompatibility == AttackCompatibility.Melee && birthAttackType == AttackType.Melee) ||
                (node.attackCompatibility == AttackCompatibility.Ranged && birthAttackType == AttackType.Ranged));
        }

        // Ordered options belong to the individual and survive save/load and respawn.
        public List<string> availableSkillIDs = new List<string>();
        public bool talentsGenerated;
        public List<TalentOption> talentOptions = new List<TalentOption>();
        [System.NonSerialized] private Dictionary<string, SkillNodeSO> _runtimeOptions;

        public List<SkillNodeSO> ResolveTalents(MonsterData data, int maxLevel = 0)
        {
            var catalog = new List<SkillNodeSO>();
            if (data == null) return catalog;
            EnsureBirthAttack(data);
            void Add(IEnumerable<SkillNodeSO> source)
            {
                if (source == null) return;
                foreach (var node in source)
                    if (node != null && !string.IsNullOrEmpty(node.skillID) &&
                        !catalog.Exists(n => n.skillID == node.skillID)) catalog.Add(node);
            }
            Add(data.globalSkillPool);
            Add(data.availableSkills);
            if (catalog.Count == 0) Add(Resources.LoadAll<SkillNodeSO>("Nodes"));
            var birth = GetBirthAttack(data);
            if (birth != null) Add(birth.upgradePool);
            Add(Resources.LoadAll<SkillNodeSO>("CombatUpgrades"));
            availableSkillIDs ??= new List<string>();
            unlockedSkillIDs ??= new List<string>();
            claimedRewards ??= new List<ClaimedReward>();
            talentOptions ??= new List<TalentOption>();
            _runtimeOptions ??= new Dictionary<string, SkillNodeSO>();
            if (maxLevel <= 0) maxLevel = data.LevelCap;
            if (talentOptions.Count == 0 && availableSkillIDs.Count == 0 && unlockedSkillIDs.Count > 0)
                foreach (var node in catalog) availableSkillIDs.Add(node.skillID);

            // Keep the old options and their purchased IDs when loading an older save.
            if (talentOptions.Count == 0 && availableSkillIDs.Count > 0)
            {
                int level = 1;
                for (int i = 0; i < availableSkillIDs.Count; i += 2)
                {
                    level++;
                    for (int side = 0; side < 2 && i + side < availableSkillIDs.Count; side++)
                    {
                        var source = catalog.Find(n => n.skillID == availableSkillIDs[i + side]);
                        if (source != null) level = Mathf.Max(level, source.requiredMonsterLevel);
                    }
                    for (int side = 0; side < 2; side++)
                    {
                        string id = i + side < availableSkillIDs.Count ? availableSkillIDs[i + side] : "";
                        talentOptions.Add(new TalentOption { optionID = id, sourceSkillID = id, requiredLevel = level });
                    }
                }
            }
            // Only unclaimed legacy milestone rows are migrated. Purchases and option IDs survive.
            var upgrades = birth != null && birth.upgradePool != null && birth.upgradePool.Count > 0
                ? birth.upgradePool.FindAll(n => n != null && n.rewardType == RewardType.SkillUpgrade && SupportsNode(n))
                : catalog.FindAll(n => n.rewardType == RewardType.SkillUpgrade && SupportsNode(n));
            var modifiers = catalog.FindAll(n => n.rewardType == RewardType.ModifierChoice && SupportsNode(n));
            bool hasNativeSkill = birthAttackType == AttackType.Melee
                ? (birth != null ? birth.meleeData : data.meleeData) != null
                : (birth != null ? birth.projectileData : data.projectileData)?.prefab != null;
            if (combatTreeVersion < 1)
            {
                foreach (var option in talentOptions)
                {
                    if (!hasNativeSkill || option.requiredLevel % 5 != 0 || IsLevelRewardClaimed(option.requiredLevel) ||
                        talentOptions.Exists(o => o.requiredLevel == option.requiredLevel && IsSkillUnlocked(o.optionID))) continue;
                    var source = RollTalent(upgrades, option.requiredLevel);
                    option.sourceSkillID = source != null ? source.skillID : "";
                    _runtimeOptions.Remove(option.optionID);
                }
                combatTreeVersion = 1;
            }
            // Sampling with replacement keeps the individual's choices stable after generation.
            int nextLevel = talentOptions.Count == 0 ? 2 : talentOptions[talentOptions.Count - 1].requiredLevel + 1;
            if (modifiers.Exists(n => n.rollWeight > 0) || upgrades.Exists(n => n.rollWeight > 0))
                for (int level = nextLevel; level <= maxLevel; level++)
                    for (int side = 0; side < 2; side++)
                    {
                        var rowPool = hasNativeSkill && level % 5 == 0 ? upgrades : modifiers;
                        if (side == 1 && talentOptions.Count > 0)
                        {
                            string leftID = talentOptions[talentOptions.Count - 1].sourceSkillID;
                            var alternatives = rowPool.FindAll(n => n.skillID != leftID);
                            rowPool = alternatives;
                        }
                        SkillNodeSO source = RollTalent(rowPool, level);
                        talentOptions.Add(new TalentOption {
                            optionID = System.Guid.NewGuid().ToString("N"),
                            sourceSkillID = source != null ? source.skillID : "",
                            requiredLevel = level
                        });
                    }
            talentsGenerated = talentOptions.Count > 0;
            var result = new List<SkillNodeSO>();
            const string rootID = "birth-attack";
            if (!_runtimeOptions.TryGetValue(rootID, out var birthNode))
            {
                birthNode = ScriptableObject.CreateInstance<SkillNodeSO>();
                birthNode.hideFlags = HideFlags.DontSave;
                birthNode.skillID = rootID;
                birthNode.skillName = birth != null ? birth.displayName : birthAttackType.ToString();
                birthNode.description = birth != null ? birth.description :
                    "Native combat specialization. Determined at birth.";
                birthNode.icon = birth != null && birth.icon != null ? birth.icon : data.icon;
                birthNode.rewardType = RewardType.SkillUnlock;
                birthNode.requiredMonsterLevel = 1;
                birthNode.skillPointCost = 0;
                _runtimeOptions.Add(rootID, birthNode);
            }
            ClaimReward(1, rootID);
            result.Add(birthNode);
            result.Add(null); // A single node in the first row; later rows remain pairs.
            foreach (var option in talentOptions)
            {
                var source = catalog.Find(n => n.skillID == option.sourceSkillID);
                if (source == null) { result.Add(null); continue; }
                if (!_runtimeOptions.TryGetValue(option.optionID, out var node))
                {
                    node = Object.Instantiate(source);
                    node.hideFlags = HideFlags.DontSave;
                    node.skillID = option.optionID;
                    node.requiredMonsterLevel = option.requiredLevel;
                    _runtimeOptions.Add(option.optionID, node);
                }
                result.Add(node);
            }
            return result;
        }

        private static SkillNodeSO RollTalent(List<SkillNodeSO> catalog, int level)
        {
            double total = 0;
            foreach (var node in catalog)
                if (node.requiredMonsterLevel <= level) total += Mathf.Max(0, node.rollWeight);
            if (total <= 0) return null;
            double roll = Random.value * total;
            SkillNodeSO last = null;
            foreach (var node in catalog)
            {
                if (node.rollWeight <= 0 || node.requiredMonsterLevel > level) continue;
                last = node;
                roll -= node.rollWeight;
                if (roll < 0) return node;
            }
            return last;
        }

        public List<string> unlockedSkillIDs = new List<string>();
        public List<ClaimedReward> claimedRewards = new List<ClaimedReward>();

        public bool IsSkillUnlocked(string skillID)
        {
            return unlockedSkillIDs != null && unlockedSkillIDs.Contains(skillID);
        }

        public bool IsLevelRewardClaimed(int level)
        {
            return claimedRewards != null && claimedRewards.Exists(r => r.levelUnlocked == level);
        }

        public void ClaimReward(int level, string skillID)
        {
            if (string.IsNullOrEmpty(skillID)) return;

            if (!unlockedSkillIDs.Contains(skillID))
            {
                unlockedSkillIDs.Add(skillID);
            }

            ClaimedReward reward = claimedRewards.Find(r => r.levelUnlocked == level);
            if (reward == null)
            {
                claimedRewards.Add(new ClaimedReward
                {
                    levelUnlocked = level,
                    chosenOptionID = skillID
                });
            }
            else
            {
                reward.chosenOptionID = skillID;
            }
        }

        public int GetAvailablePoints(List<SkillNodeSO> availableSkills)
        {
            int totalEarned = Mathf.Max(0, currentLevel - 1);
            int spent = 0;

            if (unlockedSkillIDs != null && availableSkills != null)
            {
                foreach (string id in unlockedSkillIDs)
                {
                    SkillNodeSO node = availableSkills.Find(n => n != null && n.skillID == id);
                    if (node != null) spent += node.skillPointCost;
                }
            }

            return Mathf.Max(0, totalEarned - spent);
        }
    }
}
