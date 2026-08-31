using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterSkillTree : MonoBehaviour
    {
        [Header("Configuração do Monstro")]
        [SerializeField] private List<SkillNodeSO> _availableNodes = new List<SkillNodeSO>();


        public List<SkillNodeSO> AvailableNodes => _availableNodes;
        private Monster _monster;
        private List<string> _unlockedSkillIDs = new List<string>();

        public int AvailableSkillPoints { get; private set; }

        public event Action OnSkillTreeUpdated;
        // 🎯 DECLARE ESTA LINHA SE ELA NÃO EXISTIR:

        private MonsterData _data;

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
            foreach (string id in _unlockedSkillIDs)
            {
                SkillNodeSO node = _availableNodes.Find(n => n.skillID == id);
                if (node != null) spent += node.skillPointCost;
            }
            return spent;
        }


        public void InitializeTree(MonsterData data)
        {
            _data = data;
            ApplySkillModifiers();
        }

        public bool CanUnlockNode(SkillNodeSO node, MonsterData data)
        {
            // 1. Validações básicas de referência
            if (node == null || data == null) return false;

            if (data.unlockedSkillIDs == null)
                data.unlockedSkillIDs = new List<string>();

            // 2. Pontos e Nível vindos diretamente do MonsterData
            int currentLevel = data.currentLevel;
            int unspentPoints = data.GetAvailablePoints();

            // 3. Requisitos base (Nível, Pontos e não comprado)
            bool hasEnoughLevel = currentLevel >= node.requiredMonsterLevel;
            bool hasSkillPoints = unspentPoints >= node.skillPointCost;
            bool notAlreadyUnlocked = !data.unlockedSkillIDs.Contains(node.skillID);

            // 4. Trava de Exclusão Mútua: Checa se a opção rival da mesma linha JÁ foi comprada
            bool mutuallyExclusiveNotUnlocked = true;
            if (node.mutuallyExclusiveSkill != null)
            {
                mutuallyExclusiveNotUnlocked = !data.unlockedSkillIDs.Contains(node.mutuallyExclusiveSkill.skillID);
            }

            // 5. Valida a habilidade pré-requisito (Pai)
            bool parentRequirementMet = true;
            if (node.requiredParentSkill != null)
            {
                parentRequirementMet = data.unlockedSkillIDs.Contains(node.requiredParentSkill.skillID);
            }

            // Retorna true apenas se TODAS as condições forem atendidas
            return hasEnoughLevel && 
                hasSkillPoints && 
                notAlreadyUnlocked && 
                mutuallyExclusiveNotUnlocked && 
                parentRequirementMet;
        }

        public bool TryUnlockNode(SkillNodeSO node)
        {
            if (!CanUnlockNode(node, _data)) return false;

            _unlockedSkillIDs.Add(node.skillID);
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
            foreach (string id in _unlockedSkillIDs)
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
            return new List<string>(_unlockedSkillIDs);
        }

        public void RestoreSkills(List<string> savedSkillIDs)
        {
            if (savedSkillIDs == null) return;

            _unlockedSkillIDs = new List<string>(savedSkillIDs);
            RecalculateAvailablePoints();
            ApplySkillModifiers();
            
            OnSkillTreeUpdated?.Invoke();
        }
        public bool IsNodeUnlocked(string skillID)
        {
            if (string.IsNullOrEmpty(skillID)) return false;
            return _unlockedSkillIDs != null && _unlockedSkillIDs.Contains(skillID);
        }
    }
}