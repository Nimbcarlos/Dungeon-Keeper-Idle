using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class UI_MonsterCollectionPage : MonoBehaviour
    {
        [Header("Lista")]
        [SerializeField] private Transform _content;
        [SerializeField] private UI_MonsterListItem _cardPrefab;
        [SerializeField] private GameObject _emptyText;

        [Header("Ficha")]
        [SerializeField] private GameObject _detailsPanel;
        [SerializeField] private Image _portrait;

        [Header("Prévia Animada")]
        [SerializeField] private UI_MonsterDisplay _monsterDisplay;

        private MonsterData _displayedData;

        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _summaryText;
        [SerializeField] private UI_MonsterStatsPanel _statsView;

        [Header("Abas da Ficha")]
        [SerializeField] private Button _statsButton;
        [SerializeField] private Button _talentsButton;
        [SerializeField] private GameObject _statsPanel;
        [SerializeField] private GameObject _talentsPanel;
        [Header("Árvore de Talentos")]
        [SerializeField] private UI_SkillTreeWindow _skillTreeView;


        private readonly List<UI_MonsterListItem> _cards = new();
        private readonly List<MonsterInstance> _cardInstances = new();

        private InventoryManager _inventory;
        private MonsterInstance _selected;

        private void Awake()
        {
            if (_statsButton != null)
                _statsButton.onClick.AddListener(ShowStats);

            if (_talentsButton != null)
                _talentsButton.onClick.AddListener(ShowTalents);

            ShowStats();
        }

        private void OnEnable()
        {
            ConnectInventory();
        }

        private void Start()
        {
            // Segunda tentativa caso o InventoryManager ainda
            // não tivesse executado Awake durante OnEnable.
            if (_inventory == null)
                ConnectInventory();
        }

        private void OnDisable()
        {
            DisconnectInventory();

            if (_monsterDisplay != null)
                _monsterDisplay.ClearDisplay();

            _displayedData = null;
        }


        private void OnDestroy()
        {
            DisconnectInventory();

            if (_statsButton != null)
                _statsButton.onClick.RemoveListener(ShowStats);

            if (_talentsButton != null)
                _talentsButton.onClick.RemoveListener(ShowTalents);
        }

        private void ConnectInventory()
        {
            DisconnectInventory();

            _inventory = InventoryManager.Instance;

            if (_inventory != null)
                _inventory.OnInventoryChanged += RefreshList;

            RefreshList();
        }

        private void DisconnectInventory()
        {
            if (_inventory != null)
                _inventory.OnInventoryChanged -= RefreshList;

            _inventory = null;
        }

        public void RefreshList()
        {
            string selectedID = _selected != null
                ? _selected.instanceID
                : null;

            ClearCards();
            _selected = null;

            if (_inventory == null)
            {
                UpdateSelection();
                return;
            }

            if (_content == null || _cardPrefab == null)
            {
                Debug.LogError(
                    "[Collection] Atribua Content e Card Prefab.",
                    this);

                UpdateSelection();
                return;
            }

            MonsterDatabase database = _inventory.GetDatabase();

            if (database == null)
            {
                Debug.LogError(
                    "[Collection] InventoryManager sem MonsterDatabase.",
                    this);

                UpdateSelection();
                return;
            }

            foreach (MonsterInstance instance in _inventory.OwnedInstances)
            {
                if (instance == null) continue;

                UI_MonsterListItem card =
                    Instantiate(_cardPrefab, _content);

                card.Setup(
                    instance,
                    database,
                    SelectMonster,
                    SelectMonster);

                _cards.Add(card);
                _cardInstances.Add(instance);

                if (instance.instanceID == selectedID)
                    _selected = instance;
            }

            if (_selected == null && _cardInstances.Count > 0)
                _selected = _cardInstances[0];

            UpdateSelection();
        }

        private void ClearCards()
        {
            foreach (UI_MonsterListItem card in _cards)
            {
                if (card == null) continue;

                // Remove imediatamente do layout antes do Destroy.
                card.gameObject.SetActive(false);
                Destroy(card.gameObject);
            }

            _cards.Clear();
            _cardInstances.Clear();
        }

        private void SelectMonster(MonsterInstance instance)
        {
            _selected = instance;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (_emptyText != null)
                _emptyText.SetActive(_cards.Count == 0);

            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] == null) continue;

                bool selected = _selected != null &&
                    _cardInstances[i].instanceID == _selected.instanceID;

                _cards[i].SetSelected(selected);
            }

            RefreshDetails();
        }

        // Pode ser chamado depois de uma alteração de progressão.
        public void RefreshSelectedMonster()
        {
            foreach (UI_MonsterListItem card in _cards)
            {
                if (card != null)
                    card.Refresh();
            }

            RefreshDetails();
        }

        private void RefreshDetails()
        {
            MonsterData data = _selected != null && _inventory != null
                ? _selected.GetData(_inventory.GetDatabase())
                : null;

            if (_detailsPanel != null)
                _detailsPanel.SetActive(data != null);

            if (data == null)
            {
                if (_monsterDisplay != null)
                    _monsterDisplay.ClearDisplay();

                _displayedData = null;
                return;
            }

            if (_monsterDisplay != null && _displayedData != data)
            {
                _monsterDisplay.DisplayMonster(data);
                _displayedData = data;
            }


            int level = _selected.progression != null
                ? _selected.progression.currentLevel
                : 1;

            int xp = _selected.progression != null
                ? _selected.progression.currentXP
                : 0;

            if (_portrait != null)
            {
                _portrait.sprite = data.icon;
                _portrait.preserveAspect = true;
            }

            if (_nameText != null)
                _nameText.text = data.displayName;

            if (_summaryText != null)
            {
                _summaryText.text =
                    $"{_selected.quality} · Lv. {level}\n{xp} XP";
            }

            if (_statsView != null)
                _statsView.Display(data.GetStatsForLevel(level));
        }

        public void ShowStats()
        {
            if (_skillTreeView != null) _skillTreeView.CloseWindow();
            SetDetailsTab(false);
        }

        public void ShowTalents()
        {
            SetDetailsTab(true);
        }

        private void SetDetailsTab(bool talents)
        {
            if (_statsPanel != null)
                _statsPanel.SetActive(!talents);

            if (_talentsPanel != null)
                _talentsPanel.SetActive(talents);

            if (_statsButton != null)
                _statsButton.interactable = talents;

            if (_talentsButton != null)
                _talentsButton.interactable = !talents;
        }
    }
}
