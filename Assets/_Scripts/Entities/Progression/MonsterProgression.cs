using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
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