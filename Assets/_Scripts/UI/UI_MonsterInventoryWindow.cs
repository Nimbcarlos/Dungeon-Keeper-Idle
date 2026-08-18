using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField] private TextMeshProUGUI _previewHpText;
        [SerializeField] private TextMeshProUGUI _previewAttackText;
        [SerializeField] private TextMeshProUGUI _previewSpeedText;
        [SerializeField] private TextMeshProUGUI _instructionText;

        [Header("Lado Direito - Grid de Seleção")]
        [SerializeField] private Transform _gridContentParent;
        [SerializeField] private GameObject _monsterItemPrefab;

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

            // MUDANÇA CRÍTICA: Em vez de carregar todos da pasta, puxa apenas os desbloqueados do InventoryManager!
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

            InventoryManager.Instance?.RequestEquipMonster(targetSlot, _currentlySelectedMonster);
            
            // Atualiza a lista filtrada
            List<MonsterData> availableMonsters = InventoryManager.Instance != null 
                ? InventoryManager.Instance.GetUnlockedMonstersList() 
                : new List<MonsterData>();

            PopulateGrid(availableMonsters.ToArray());
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

            // Atualiza o monstro animado na UI!
            if (_monsterDisplay != null)
            {
                _monsterDisplay.DisplayMonster(monster);
            }

            if (_previewNameText != null) _previewNameText.text = monster.displayName;
            if (_previewHpText != null) _previewHpText.text = $"HP: {monster.stats.maxHP}";
            if (_previewAttackText != null) _previewAttackText.text = $"ATK: {monster.stats.attackPower}";
            if (_previewSpeedText != null) _previewSpeedText.text = $"SPD: {monster.stats.moveSpeed}";

            if (_instructionText != null) 
                _instructionText.text = "Click on a lane on the map to place this monster!";
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
    }
}