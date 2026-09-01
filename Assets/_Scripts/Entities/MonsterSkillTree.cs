using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterSkillTree : MonoBehaviour
    {
        [Header("Configuração do Monstro")]
        [SerializeField] private List<SkillNodeSO> _availableNodes = new List<SkillNodeSO>();

        private MonsterData _data;
        private Monster _monster;
        private List<string> _unlockedSkillIDs = new List<string>();

        public event Action OnSkillTreeUpdated;
        public int AvailableSkillPoints { get; private set; }

        // 🎯 BUSCA DINÂMICA COM AUTO-RECUPERAÇÃO DO DATA
        public List<SkillNodeSO> AvailableNodes
        {
            get
            {
                // Se o _data ainda não foi injetado via InitializeTree, tenta pegar do Monster
                if (_data == null && _monster != null)
                {
                    _data = _monster.Data;
                    Debug.Log($"[MonsterSkillTree] Auto-recuperando _data no getter através do Monster. Result: {(_data != null ? _data.name : "NULL")}");
                }

                // Se a lista local estiver vazia, retorna a lista do MonsterData
                if ((_availableNodes == null || _availableNodes.Count == 0) && _data != null)
                {
                    Debug.Log($"[MonsterSkillTree] Retornando {_data.availableSkills.Count} nó(s) vindos do MonsterData ({_data.displayName})");
                    return _data.availableSkills;
                }

                Debug.Log($"[MonsterSkillTree] Retornando {_availableNodes.Count} nó(s) da lista local _availableNodes");
                return _availableNodes;
            }
        }

        private void Awake()
        {
            _monster = GetComponent<Monster>();
            
            // Tenta vincular o Data já no Awake se o Monster tiver referência
            if (_monster != null && _monster.Data != null)
            {
                InitializeTree(_monster.Data);
            }
        }

        public void InitializeTree(MonsterData data)
        {
            _data = data;
            Debug.Log($"[MonsterSkillTree] InitializeTree executado para monstro: {(data != null ? data.displayName : "NULL")}");

            if (_data != null)
            {
                if ((_availableNodes == null || _availableNodes.Count == 0))
                {
                    _availableNodes = _data.availableSkills;
                    Debug.Log($"[MonsterSkillTree] Copiado {_availableNodes.Count} nó(s) do MonsterData para _availableNodes");
                }

                if (_data.unlockedSkillIDs != null)
                {
                    _unlockedSkillIDs = new List<string>(_data.unlockedSkillIDs);
                }
            }

            RecalculateAvailablePoints();
            ApplySkillModifiers();
        }

        public void OnLevelUp(int newLevel)
        {
            RecalculateAvailablePoints();
            OnSkillTreeUpdated?.Invoke();
        }

        public void RecalculateAvailablePoints()
        {
            if (_monster == null) _monster = GetComponent<Monster>();
            if (_monster == null) return;

            int totalEarnedPoints = Mathf.Max(0, _monster.CurrentLevel - 1);
            int spentPoints = GetSpentPoints();

            AvailableSkillPoints = Mathf.Max(0, totalEarnedPoints - spentPoints);
        }

        private int GetSpentPoints()
        {
            int spent = 0;
            List<SkillNodeSO> currentNodes = AvailableNodes;

            foreach (string id in _unlockedSkillIDs)
            {
                SkillNodeSO node = currentNodes.Find(n => n != null && n.skillID == id);
                if (node != null) spent += node.skillPointCost;
            }
            return spent;
        }

        public bool CanUnlockNode(SkillNodeSO node, MonsterData data)
        {
            if (node == null || data == null) return false;

            if (data.unlockedSkillIDs == null)
                data.unlockedSkillIDs = new List<string>();

            int currentLevel = data.currentLevel;
            int unspentPoints = data.GetAvailablePoints();

            // 1. Checagem de Nível Mínimo e Pontos
            bool hasEnoughLevel = currentLevel >= node.requiredMonsterLevel;
            bool hasSkillPoints = unspentPoints >= node.skillPointCost;
            bool notAlreadyUnlocked = !data.unlockedSkillIDs.Contains(node.skillID);

            // 2. Trava de Exclusão Mútua Rigorosa (Checa se o nó rival da mesma linha já foi comprado)
            bool mutuallyExclusiveNotUnlocked = true;
            if (node.mutuallyExclusiveSkill != null)
            {
                bool rivalUnlockedInData = data.unlockedSkillIDs.Contains(node.mutuallyExclusiveSkill.skillID);
                bool rivalUnlockedInLocal = _unlockedSkillIDs.Contains(node.mutuallyExclusiveSkill.skillID);
                
                if (rivalUnlockedInData || rivalUnlockedInLocal)
                {
                    mutuallyExclusiveNotUnlocked = false; // BLOQUEIA!
                }
            }

            // 3. Checagem do Nó Pai
            bool parentRequirementMet = true;
            if (node.requiredParentSkill != null)
            {
                parentRequirementMet = data.unlockedSkillIDs.Contains(node.requiredParentSkill.skillID);
            }

            return hasEnoughLevel && 
                hasSkillPoints && 
                notAlreadyUnlocked && 
                mutuallyExclusiveNotUnlocked && 
                parentRequirementMet;
        }

        public bool TryUnlockNode(SkillNodeSO node, SkillNodeSO oppositeNodeInRow = null)
        {
            if (_data == null && _monster != null) _data = _monster.Data;

            // 🎯 Chama a validação da linha passando o nó oposto!
            if (!CanUnlockNodeInRow(node, oppositeNodeInRow)) return false;

            _unlockedSkillIDs.Add(node.skillID);
            
            if (_data != null && !_data.unlockedSkillIDs.Contains(node.skillID))
            {
                _data.unlockedSkillIDs.Add(node.skillID);
            }

            RecalculateAvailablePoints();
            ApplySkillModifiers();

            OnSkillTreeUpdated?.Invoke();
            Debug.Log($"🌳 Habilidade '{node.skillName}' desbloqueada para {gameObject.name}!");
            return true;
        }

        public bool CanUnlockNodeInRow(SkillNodeSO clickedNode, SkillNodeSO oppositeNodeInRow)
        {
            if (clickedNode == null || _data == null) return false;

            // 1. Checa se o monstro tem nível suficiente para esta linha
            if (_monster.CurrentLevel < clickedNode.requiredMonsterLevel) return false;

            // 2. Se o nó clicado já foi comprado, bloqueia
            if (_data.IsSkillUnlocked(clickedNode.skillID)) return false;

            // 3. EXCLUSÃO MÚTUA DA LINHA: Se o nó oposto (par da mesma linha) já foi comprado no Data, BLOQUEIA este!
            if (oppositeNodeInRow != null && _data.IsSkillUnlocked(oppositeNodeInRow.skillID))
            {
                return false; // Trava! O jogador já escolheu o outro nó dessa mesma linha de nível.
            }

            // 4. Checa se o monstro ainda possui pontos disponíveis
            return _data.GetAvailablePoints() >= clickedNode.skillPointCost;
        }

        public void ApplySkillModifiers()
        {
            if (_monster == null) return;

            List<SkillNodeSO> nodes = AvailableNodes;

            foreach (string id in _unlockedSkillIDs)
            {
                SkillNodeSO node = nodes.Find(n => n != null && n.skillID == id);
                if (node == null) continue;

                switch (node.skillType)
                {
                    case SkillType.FlatHealth:
                        _monster.Health?.ModifyMaxHealth(node.modifierValue);
                        break;
                }
            }
        }

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