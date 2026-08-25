using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterSkillTree : MonoBehaviour
    {
        [Header("Configuração do Monstro")]
        [SerializeField] private List<SkillNodeSO> _availableNodes = new List<SkillNodeSO>();

        private Monster _monster;
        private List<string> _unlockedNodeIDs = new List<string>();

        public int AvailableSkillPoints { get; private set; }
        public event Action OnSkillTreeUpdated;

        private void Awake()
        {
            _monster = GetComponent<Monster>();
        }

        /// <summary>
        /// Atualiza os pontos disponíveis quando o monstro sobe de nível
        /// </summary>
        public void OnLevelUp(int newLevel)
        {
            RecalculateAvailablePoints();
            OnSkillTreeUpdated?.Invoke();
        }

        public void RecalculateAvailablePoints()
        {
            if (_monster == null) return;
            
            // Concede 1 ponto por nível (ex: Nível 5 = 4 pontos totais ganhos)
            int totalEarnedPoints = Mathf.Max(0, _monster.CurrentLevel - 1);
            int spentPoints = GetSpentPoints();

            AvailableSkillPoints = Mathf.Max(0, totalEarnedPoints - spentPoints);
        }

        private int GetSpentPoints()
        {
            int spent = 0;
            foreach (string id in _unlockedNodeIDs)
            {
                SkillNodeSO node = _availableNodes.Find(n => n.skillID == id);
                if (node != null) spent += node.skillPointCost;
            }
            return spent;
        }

        public bool CanUnlockNode(SkillNodeSO node)
        {
            if (node == null || _unlockedNodeIDs.Contains(node.skillID)) return false;
            if (_monster.CurrentLevel < node.requiredMonsterLevel) return false;
            if (AvailableSkillPoints < node.skillPointCost) return false;

            // Verifica se o nó pré-requisito foi comprado
            if (node.requiredParentSkill != null && !_unlockedNodeIDs.Contains(node.requiredParentSkill.skillID))
                return false;

            return true;
        }

        public bool TryUnlockNode(SkillNodeSO node)
        {
            if (!CanUnlockNode(node)) return false;

            _unlockedNodeIDs.Add(node.skillID);
            RecalculateAvailablePoints();
            ApplySkillModifiers();

            OnSkillTreeUpdated?.Invoke();
            Debug.Log($"🌳 Habilidade '{node.skillName}' desbloqueada para {_monster.name}!");
            return true;
        }

        public void ApplySkillModifiers()
        {
            if (_monster == null) return;

            // Aqui você recarrega os status base do nível e soma os bônus ativados da SkillTree
            foreach (string id in _unlockedNodeIDs)
            {
                SkillNodeSO node = _availableNodes.Find(n => n.skillID == id);
                if (node == null) continue;

                switch (node.skillType)
                {
                    case SkillType.FlatHealth:
                        _monster.Health?.ModifyMaxHealth(node.modifierValue);
                        break;
                    case SkillType.PercentDamage:
                        // Aplica multiplicador de dano no Monster/Character
                        break;
                }
            }
        }

        // ── SAVE & LOAD INTEGRATION ──

        public List<string> GetUnlockedSkillIDs()
        {
            return new List<string>(_unlockedNodeIDs);
        }

        public void RestoreSkills(List<string> savedSkillIDs)
        {
            if (savedSkillIDs == null) return;

            _unlockedNodeIDs = new List<string>(savedSkillIDs);
            RecalculateAvailablePoints();
            ApplySkillModifiers();
            
            OnSkillTreeUpdated?.Invoke();
        }
    }
}