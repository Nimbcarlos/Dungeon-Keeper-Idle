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

        public event Action OnSkillTreeUpdated;

        public MonsterProgression Progression => _progression;

        public IReadOnlyList<SkillNodeSO> AvailableNodes
        {
            get
            {
                return _data != null ? _data.availableSkills : Array.Empty<SkillNodeSO>();
            }
        }

        public int AvailableSkillPoints
        {
            get
            {
                if (_progression == null || _data == null) return 0;
                return _progression.GetAvailablePoints(_data.availableSkills);
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


            ApplySkillModifiers();
            OnSkillTreeUpdated?.Invoke();
        }

        public void OnLevelUp(int newLevel)
        {
            if (_progression != null)
            {
                _progression.currentLevel = newLevel;
            }

            OnSkillTreeUpdated?.Invoke();
        }

        public bool CanUnlockNodeInRow(SkillNodeSO clickedNode, SkillNodeSO oppositeNodeInRow)
        {
            if (clickedNode == null || _data == null || _progression == null) return false;
            if (_data.availableSkills == null) return false;


            // 1. Checa nível e se já foi comprado
            if (_monster.CurrentLevel < clickedNode.requiredMonsterLevel) return false;
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
            _progression.ClaimReward(node.requiredMonsterLevel, node.skillID);

            ApplySkillModifiers();
            OnSkillTreeUpdated?.Invoke();

            Debug.Log($"🌳 Habilidade '{node.skillName}' desbloqueada para {_monster.name}!");
            return true;
        }

        public void ApplySkillModifiers()
        {
            if (_monster == null || _data == null || _progression == null) return;

            TryRecalculateBaseStats(_monster);

            foreach (string skillID in _progression.unlockedSkillIDs)
            {
                SkillNodeSO node = _data.availableSkills.Find(n => n != null && n.skillID == skillID);
                if (node == null) continue;

                switch (node.rewardType)
                {
                    case RewardType.ModifierChoice:
                        ApplyStatusModifier(node);
                        break;

                    case RewardType.SkillUpgrade:
                        ApplyCombatSkillUpgrade(node);
                        break;
                }
            }
        }

        private void TryRecalculateBaseStats(Monster monster)
        {
            if (monster == null) return;

            var method = monster.GetType().GetMethod(
                "RecalculateBaseStats",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            method?.Invoke(monster, null);
        }

        private void ApplyStatusModifier(SkillNodeSO node)
        {
            switch (node.skillType)
            {
                case SkillType.FlatHealth:
                    _monster.Health?.ModifyMaxHealth(node.modifierValue);
                    break;

                case SkillType.PercentDamage:
                    _monster.Stats.attackPower += Mathf.RoundToInt(_monster.Stats.attackPower * (node.modifierValue / 100f));
                    break;

                case SkillType.AttackSpeed:
                    _monster.Stats.attackSpeed += node.modifierValue;
                    break;
            }
        }

        private void ApplyCombatSkillUpgrade(SkillNodeSO node)
        {
            MeleeSkill melee = GetComponent<MeleeSkill>();
            if (melee != null)
            {
                melee.ApplyUpgradeBonus(
                    extraDamage: Mathf.RoundToInt(node.modifierValue), 
                    extraRange: 0f, 
                    extraCleave: 0, 
                    cdr: 0f
                );
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
            ApplySkillModifiers();
            OnSkillTreeUpdated?.Invoke();
        }
    }
}