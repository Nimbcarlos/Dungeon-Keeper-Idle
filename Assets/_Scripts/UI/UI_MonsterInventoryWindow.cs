using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonKeeper
{
    public class UI_MonsterInventoryWindow : MonoBehaviour
    {
        public static UI_MonsterInventoryWindow Instance { get; private set; }

        [Header("Lado Esquerdo - Informações do Monstro Selecionado")]
        [Header("Exibição Animada")]
        [SerializeField] private UI_MonsterDisplay _monsterDisplay;

        [SerializeField] private TextMeshProUGUI _previewNameText;
        [SerializeField] private TextMeshProUGUI _previewLevelText;  // NOVO: Exibe ex: "LVL: 5" ou "LVL: 25 (MAX)"
        [SerializeField] private TextMeshProUGUI _previewXpText;     // NOVO: Exibe ex: "XP: 120 / 250"
        [SerializeField] private TextMeshProUGUI _previewHpText;
        [SerializeField] private TextMeshProUGUI _previewAttackText;
        [SerializeField] private TextMeshProUGUI _previewSpeedText;
        [SerializeField] private TextMeshProUGUI _instructionText;

        [Header("Lado Direito - Grid de Seleção")]
        [SerializeField] private Transform _gridContentParent;
        [SerializeField] private GameObject _monsterItemPrefab;

        [Header("Ações do Monstro Selecionado")]
        [SerializeField] private Button _openSkillTreeButton;

        private MonsterData _currentlySelectedMonster;

        public bool IsOpen => gameObject.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;

            gameObject.SetActive(false);
        }

        public void ToggleWindow()
        {
            if (IsOpen) CloseWindow();
            else OpenWindow();
        }

        public void OpenWindow()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;

            ToggleAllLaneHighlights(true);

            List<MonsterData> availableMonsters = InventoryManager.Instance != null 
                ? InventoryManager.Instance.GetUnlockedMonstersList() 
                : new List<MonsterData>();

            PopulateGrid(availableMonsters.ToArray());

            if (availableMonsters.Count > 0)
            {
                SelectMonster(availableMonsters[0]);
            }
        }

        public void AssignSelectedMonsterToLane(MonsterSlot targetSlot)
        {
            if (_currentlySelectedMonster == null || targetSlot == null) return;

            // 1. Repassa a tentativa para o InventoryManager
            InventoryManager.Instance?.RequestEquipMonster(targetSlot, _currentlySelectedMonster);
            
            // 2. Reconstrução do Grid e da Seleção
            List<MonsterData> availableMonsters = InventoryManager.Instance != null 
                ? InventoryManager.Instance.GetUnlockedMonstersList() 
                : new List<MonsterData>();

            PopulateGrid(availableMonsters.ToArray());

            // 3. Força a atualização do painel do monstro selecionado (para reabilitar a Skill Tree/botões)
            SelectMonster(_currentlySelectedMonster);
        }
        /*
        public void AssignSelectedMonsterToLane(MonsterSlot targetSlot)
        {
            if (_currentlySelectedMonster == null || targetSlot == null) return;

            // CENÁRIO 1: O monstro selecionado JÁ ESTÁ exatamente nesta lane -> DESEQUIPA
            if (targetSlot.HasMonsterEquipped && targetSlot.EquippedMonsterData == _currentlySelectedMonster)
            {
                InventoryManager.Instance?.RequestUnequipMonster(targetSlot);
            }
            // CENÁRIO 2: A lane está VAZIA -> EQUIPA o monstro selecionado
            else if (!targetSlot.HasMonsterEquipped)
            {
                InventoryManager.Instance?.RequestEquipMonster(targetSlot, _currentlySelectedMonster);
            }
            // CENÁRIO 3: A lane está ocupada por OUTRO monstro -> FAZ O REPLACE
            else
            {
                InventoryManager.Instance?.RequestEquipMonster(targetSlot, _currentlySelectedMonster);
            }
            
            // Atualiza a lista visual do grid
            List<MonsterData> availableMonsters = InventoryManager.Instance != null 
                ? InventoryManager.Instance.GetUnlockedMonstersList() 
                : new List<MonsterData>();

            PopulateGrid(availableMonsters.ToArray());

            // Mantém o painel do monstro atualizado
            SelectMonster(_currentlySelectedMonster);
        }
        */

        /// <summary>
        /// Permite que scripts externos (como MonsterSlot ou Monster) selecionem o monstro diretamente na UI.
        /// </summary>
        public void SelectMonsterByData(MonsterData monsterData)
        {
            if (monsterData == null) return;

            if (!IsOpen)
            {
                OpenWindow();
            }

            SelectMonster(monsterData);
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1f;

            ToggleAllLaneHighlights(false);
        }

        private void PopulateGrid(MonsterData[] allMonsters)
        {
            foreach (Transform child in _gridContentParent)
            {
                Destroy(child.gameObject);
            }

            List<MonsterData> equippedMonsters = GetEquippedMonstersInLanes();

            foreach (MonsterData monster in allMonsters)
            {
                GameObject itemObj = Instantiate(_monsterItemPrefab, _gridContentParent);
                UI_MonsterListItem item = itemObj.GetComponent<UI_MonsterListItem>();

                bool isEquipped = equippedMonsters.Contains(monster);
                item.Setup(monster, isEquipped, SelectMonster);
            }
        }

        public void SelectMonster(MonsterData monster)
        {
            _currentlySelectedMonster = monster;

            if (_monsterDisplay != null)
            {
                _monsterDisplay.DisplayMonster(monster);
            }

            int level = monster.currentLevel;
            int xp = monster.currentXP;
            int nextLevelXP = monster.GetXPRequired(level + 1);
            bool isMaxLevel = level >= monster.LevelCap;

            var scaledStats = monster.GetStatsForLevel(level);

            if (_previewNameText != null) _previewNameText.text = monster.displayName;
            
            if (_previewLevelText != null) 
                _previewLevelText.text = isMaxLevel ? $"LVL: {level} (MAX)" : $"LVL: {level}";

            if (_previewXpText != null) 
                _previewXpText.text = isMaxLevel ? "XP: MAX" : $"XP: {xp} / {nextLevelXP}";

            if (_previewHpText != null)     _previewHpText.text = $"HP: {scaledStats.maxHP}";
            if (_previewAttackText != null) _previewAttackText.text = $"ATK: {scaledStats.attackPower}";
            if (_previewSpeedText != null)  _previewSpeedText.text = $"SPD: {scaledStats.moveSpeed}";

            if (_instructionText != null) 
                _instructionText.text = "Click on a lane on the map to place this monster!";

            // 🎯 1. Tenta achar o monstro instanciado em alguma lane
            Monster activeMonster = GetActiveMonsterInstance(monster);

            // 2. Tenta pegar a Tree da cena ou, se for null, pega direto do Prefab no MonsterData
            MonsterSkillTree skillTree = activeMonster != null 
                ? activeMonster.GetComponent<MonsterSkillTree>() 
                : (monster != null && monster.prefab != null ? monster.prefab.GetComponent<MonsterSkillTree>() : null);

            if (_openSkillTreeButton != null)
            {
                _openSkillTreeButton.onClick.RemoveAllListeners();

                // Se o monstro (ou prefab) possui a SkillTree, HABILITA o botão
                if (skillTree != null)
                {
                    _openSkillTreeButton.interactable = true;
                    _openSkillTreeButton.onClick.AddListener(() => 
                    {
                        if (activeMonster != null)
                        {
                            UI_SkillTreeWindow.Instance?.OpenWindowForMonster(activeMonster);
                        }
                        else
                        {
                            // Passa o componente da Tree obtido via prefab/data
                            UI_SkillTreeWindow.Instance?.OpenWindowForMonster(skillTree.GetComponent<Monster>());
                        }
                    });
                }
                else
                {
                    _openSkillTreeButton.interactable = false; 
                }
            }
        }

        private void ToggleAllLaneHighlights(bool visible)
        {
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);
            foreach (var slot in slots)
            {
                slot.SetHighlightVisible(visible);
            }
        }

        private List<MonsterData> GetEquippedMonstersInLanes()
        {
            List<MonsterData> equipped = new List<MonsterData>();
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);
            
            foreach (var slot in slots)
            {
                if (slot.HasMonsterEquipped)
                    equipped.Add(slot.EquippedMonsterData);
            }
            return equipped;
        }

        private Monster GetActiveMonsterInstance(MonsterData data)
        {
            if (data == null) return null;

            // 1. Busca todos os slots de lanes ativos na masmorra
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);

            foreach (var slot in slots)
            {
                // 2. Verifica se o slot possui este MonsterData equipado
                if (slot.EquippedMonsterData == data)
                {
                    // 3. Retorna o componente Monster do prefab spawnado na lane
                    return slot.GetSpawnedMonster();
                }
            }

            // Se o monstro estiver apenas no inventário e não alocado em nenhuma lane no momento
            return null;
        }
    }
}