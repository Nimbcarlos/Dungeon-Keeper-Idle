using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonKeeper
{
    public class UI_MonsterInventoryWindow : MonoBehaviour
    {
        public static UI_MonsterInventoryWindow Instance { get; private set; }

        [Header("Lado Esquerdo - Detalhes / Card Selecionado")]
        [SerializeField] private Image _previewIcon;
        [SerializeField] private TextMeshProUGUI _previewNameText;
        [SerializeField] private TextMeshProUGUI _previewHpText;
        [SerializeField] private TextMeshProUGUI _previewAttackText;
        [SerializeField] private TextMeshProUGUI _previewSpeedText;
        [SerializeField] private TextMeshProUGUI _instructionText; // Ex: "Clique em uma Lane para equipar"

        [Header("Lado Direito - Grid de Seleção")]
        [SerializeField] private Transform _gridContentParent; // Objeto "Content" dentro do ScrollView
        [SerializeField] private GameObject _monsterItemPrefab; // Prefab de UI do botãozinho

        private MonsterData _currentlySelectedMonster;

        public bool IsOpen => gameObject.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;

            gameObject.SetActive(false); // Inicia fechado por padrão
        }

        /// <summary>
        /// Método chamado pelo Botão Único do HUD ("Gerenciar Monstros")
        /// </summary>
        public void ToggleWindow()
        {
            if (IsOpen) CloseWindow();
            else OpenWindow();
        }

        public void OpenWindow()
        {
            gameObject.SetActive(true);
            
            // 1. Pausa o jogo
            Time.timeScale = 0f;

            // 2. Ativa o destaque visual (highlights) de todas as Lanes no mapa
            ToggleAllLaneHighlights(true);

            // 3. Carrega os monstros da pasta Resources/Monsters
            MonsterData[] allMonsters = Resources.LoadAll<MonsterData>("Monsters");
            PopulateGrid(allMonsters);

            if (allMonsters.Length > 0)
            {
                SelectMonster(allMonsters[0]);
            }
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);

            // 1. Retorna a velocidade do jogo ao normal
            Time.timeScale = 1f;

            // 2. Desliga os destaques das lanes
            ToggleAllLaneHighlights(false);
        }

        private void PopulateGrid(MonsterData[] allMonsters)
        {
            // Limpa a lista antiga do Content
            foreach (Transform child in _gridContentParent)
            {
                Destroy(child.gameObject);
            }

            // Descobre quais monstros já estão equipados em alguma lane
            List<MonsterData> equippedMonsters = GetEquippedMonstersInLanes();

            // Instancia os botões
            foreach (MonsterData monster in allMonsters)
            {
                GameObject itemObj = Instantiate(_monsterItemPrefab, _gridContentParent);
                UI_MonsterListItem item = itemObj.GetComponent<UI_MonsterListItem>();

                bool isEquipped = equippedMonsters.Contains(monster);

                item.Setup(monster, isEquipped, SelectMonster);
            }
        }

        /// <summary>
        /// Atualiza as informações da esquerda ao clicar em um item da lista
        /// </summary>
        public void SelectMonster(MonsterData monster)
        {
            _currentlySelectedMonster = monster;

            if (_previewIcon != null) _previewIcon.sprite = monster.icon;
            if (_previewNameText != null) _previewNameText.text = monster.displayName;
            if (_previewHpText != null) _previewHpText.text = $"HP: {monster.stats.maxHP}";
            if (_previewAttackText != null) _previewAttackText.text = $"ATK: {monster.stats.attackPower}";
            if (_previewSpeedText != null) _previewSpeedText.text = $"SPD: {monster.stats.moveSpeed}";

            if (_instructionText != null) 
                _instructionText.text = "Clique em uma Lane no mapa para posicionar este monstro!";
        }

        /// <summary>
        /// Atribui o monstro selecionado à lane clicada no mapa
        /// </summary>
        public void AssignSelectedMonsterToLane(MonsterSlot targetSlot)
        {
            if (_currentlySelectedMonster == null || targetSlot == null) return;

            targetSlot.EquipMonster(_currentlySelectedMonster);
            
            // Recarrega o grid para atualizar o estado de "Em Uso / Equipado"
            MonsterData[] allMonsters = Resources.LoadAll<MonsterData>("Monsters");
            PopulateGrid(allMonsters);
        }

        private void ToggleAllLaneHighlights(bool visible)
        {
            // CORRIGIDO: Usa a overload moderna sem FindObjectsSortMode
            MonsterSlot[] slots = FindObjectsByType<MonsterSlot>(FindObjectsInactive.Exclude);
            foreach (var slot in slots)
            {
                slot.SetHighlightVisible(visible);
            }
        }

        private List<MonsterData> GetEquippedMonstersInLanes()
        {
            List<MonsterData> equipped = new List<MonsterData>();
            // CORRIGIDO: Usa a overload moderna sem FindObjectsSortMode
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