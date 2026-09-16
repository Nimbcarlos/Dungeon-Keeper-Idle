using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterSkillTree : MonoBehaviour
    {
        private MonsterData _data;
        private MonsterProgression _progression;
        private Monster _monster;
        private List<SkillNodeSO> _availableNodes = new List<SkillNodeSO>();

        public event Action OnSkillTreeUpdated;

        public MonsterProgression Progression => _progression;

        public IReadOnlyList<SkillNodeSO> AvailableNodes
        {
            get
            {
                return _availableNodes;
            }
        }

        public int AvailableSkillPoints
        {
            get
            {
                if (_progression == null || _data == null) return 0;
                return _progression.GetAvailablePoints(_availableNodes);
            }
        }

        private void Awake()
        {
            _monster = GetComponent<Monster>();
        }

        public void InitializeTree(MonsterData data, MonsterProgression progression)
        {
            _data = data;
            _progression = progression ?? new MonsterProgression();
            _progression.unlockedSkillIDs ??= new List<string>();
            _progression.claimedRewards ??= new List<ClaimedReward>();
            _availableNodes = _progression.ResolveTalents(data, _monster != null ? _monster.MaxLevel : 0);


            ApplySkillModifiers();
            OnSkillTreeUpdated?.Invoke();
        }

        public void OnLevelUp(int newLevel)
        {
            if (_progression != null)
            {
                _progression.currentLevel = newLevel;
            }

            ApplySkillModifiers();
            OnSkillTreeUpdated?.Invoke();
        }

        public int GetRequiredLevel(SkillNodeSO node)
        {
            return node != null && _availableNodes.Contains(node) ? node.requiredMonsterLevel : int.MaxValue;
        }

        public bool CanUnlockNodeInRow(SkillNodeSO clickedNode, SkillNodeSO oppositeNodeInRow)
        {
            if (clickedNode == null || _data == null || _progression == null) return false;
            if (!_availableNodes.Contains(clickedNode)) return false;
            int index = _availableNodes.IndexOf(clickedNode);
            int oppositeIndex = index % 2 == 0 ? index + 1 : index - 1;
            oppositeNodeInRow = oppositeIndex < _availableNodes.Count ? _availableNodes[oppositeIndex] : null;


            // 1. Checa nível e se já foi comprado
            if (_progression.currentLevel < GetRequiredLevel(clickedNode)) return false;
            if (_progression.IsSkillUnlocked(clickedNode.skillID)) return false;

            // 2. Trava de Exclusão Mútua da Linha
            if (oppositeNodeInRow != null && _progression.IsSkillUnlocked(oppositeNodeInRow.skillID))
            {
                return false;
            }

            // 3. Checa pontos do estado de progressão
            return AvailableSkillPoints >= clickedNode.skillPointCost;
        }

        public bool TryUnlockNode(SkillNodeSO node, SkillNodeSO oppositeNodeInRow = null)
        {
            if (!CanUnlockNodeInRow(node, oppositeNodeInRow)) return false;

            // Altera exclusivamente o estado individual
            _progression.ClaimReward(GetRequiredLevel(node), node.skillID);

            ApplySkillModifiers();
            OnSkillTreeUpdated?.Invoke();

            Debug.Log($"🌳 Habilidade '{node.skillName}' desbloqueada para {_data.displayName}!");
            return true;
        }

        public void ApplySkillModifiers()
        {
            if (_monster == null || _data == null || _progression == null) return;
            // Rebuild from base values: re-opening the UI must not add bonuses again.
            Stats baseline = _monster.GetCombatStatsForLevel(_progression.currentLevel);
            var stats = _monster.Stats;
            if (stats == null) return;
            stats.attackPower = Mathf.RoundToInt(baseline.attackPower * (1 + GetTotalModifier(SkillType.PercentDamage) / 100f)
                + GetTotalModifier(SkillType.FlatDamage));
            stats.maxHP = Mathf.Max(1, Mathf.RoundToInt(baseline.maxHP * (1 + GetTotalModifier(SkillType.PercentHealth) / 100f)
                + GetTotalModifier(SkillType.FlatHealth)));
            stats.attackSpeed = baseline.attackSpeed + GetTotalModifier(SkillType.AttackSpeed);
            stats.moveSpeed = baseline.moveSpeed + GetTotalModifier(SkillType.MovementSpeed);
            stats.attackRange = baseline.attackRange;
            if (_monster.Health != null) _monster.Health.ModifyMaxHealth(stats.maxHP - _monster.Health.MaxHP);
            GetComponent<MeleeSkill>()?.ResetTalentUpgrades();
            GetComponent<ProjectileSkill>()?.ResetTalentUpgrades();

            foreach (string skillID in _progression.unlockedSkillIDs)
            {
                SkillNodeSO node = _availableNodes.Find(n => n != null && n.skillID == skillID);
                if (node == null) continue;

                switch (node.rewardType)
                {
                    case RewardType.SkillUpgrade:
                        ApplyCombatSkillUpgrade(node);
                        break;
                }
            }
        }

        public float GetTotalModifier(SkillType type)
        {
            float total = 0;
            if (_progression == null) return total;
            foreach (var node in _availableNodes)
                if (node != null && node.rewardType == RewardType.ModifierChoice && node.skillType == type &&
                    _progression.IsSkillUnlocked(node.skillID)) total += node.modifierValue;
            return total;
        }

        private void ApplyCombatSkillUpgrade(SkillNodeSO node)
        {
            if (!_progression.SupportsNode(node)) return;
            if (_monster.EffectiveAttackType == AttackType.Ranged)
            {
                var ranged = GetComponent<ProjectileSkill>();
                if (ranged != null && node.projectileUpgrade != null) ranged.ApplyUpgrade(node.projectileUpgrade);
            }
            else
            {
                _monster.Stats.attackRange += node.meleeRangeBonus;
                GetComponent<MeleeSkill>()?.ApplyUpgradeBonus(
                    node.meleeDamageBonus + Mathf.RoundToInt(node.modifierValue),
                    node.meleeRangeBonus, node.meleeCleaveBonus, node.meleeCooldownReduction);
            }
        }

        public bool IsNodeUnlocked(string skillID)
        {
            return _progression != null && _progression.IsSkillUnlocked(skillID);
        }

        public List<string> GetUnlockedSkillIDs()
        {
            return _progression != null ? new List<string>(_progression.unlockedSkillIDs) : new List<string>();
        }

        public void RestoreSkills(MonsterProgression progression)
        {
            if (progression == null) return;
            _progression = progression;
            _availableNodes = _progression.ResolveTalents(_data);
            ApplySkillModifiers();
            OnSkillTreeUpdated?.Invoke();
        }
    }
}
