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
        private bool _showingTalents;
        private Vector2 _layoutSize;

        private void ConfigureReadableLayout()
        {
            var page = transform as RectTransform;
            if (page == null || page.rect.width <= 0 || page.rect.height <= 0) return;
            if (_layoutSize == page.rect.size) return;
            _layoutSize = page.rect.size;
            var scroll = _content != null ? _content.GetComponentInParent<ScrollRect>(true) : null;
            if (scroll != null)
                Fit(scroll.transform as RectTransform, page, 0, .35f, 16, 16, 16, 16);
            var details = _detailsPanel != null ? _detailsPanel.transform as RectTransform : null;
            if (details == null) return;
            Fit(details, page, .35f, 1, 24, 16, 16, 16);
            // Keep the preview in the header, leaving the body for the selected tab.
            if (_monsterDisplay != null)
                Top(_monsterDisplay.transform as RectTransform, details, 0, 0, 160, 160);
            if (_nameText != null)
            {
                Top(_nameText.rectTransform, details, 180, 0, -180, 64);
                ReadableText(_nameText, 42);
            }
            if (_summaryText != null)
            {
                Top(_summaryText.rectTransform, details, 180, 68, -180, 84);
                ReadableText(_summaryText, 34);
            }
            ConfigureTabButton(_statsButton, details, 0, .49f);
            ConfigureTabButton(_talentsButton, details, .51f, 1);
            if (_statsPanel != null) Fit(_statsPanel.transform as RectTransform, details, 0, 1, 0, 0, 288, 0);
            if (_talentsPanel != null) Fit(_talentsPanel.transform as RectTransform, details, 0, 1, 0, 0, 288, 0);
            if (_statsView != null) _statsView.UseReadableLayout();
        }

        private static void ReadableText(TextMeshProUGUI text, float size)
        {
            text.enableAutoSizing = false;
            text.fontSize = size;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
        }

        private static void ConfigureTabButton(Button button, RectTransform parent, float min, float max)
        {
            if (button == null) return;
            var rect = button.transform as RectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(min, 1);
            rect.anchorMax = new Vector2(max, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -168);
            rect.sizeDelta = new Vector2(0, 108);
            foreach (var label in button.GetComponentsInChildren<TextMeshProUGUI>(true)) ReadableText(label, 38);
        }

        private static void Fit(RectTransform rect, RectTransform parent, float min, float max,
            float left, float right, float top, float bottom)
        {
            if (rect == null) return;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(min, 0);
            rect.anchorMax = new Vector2(max, 1);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            rect.localScale = Vector3.one;
        }

        private static void Top(RectTransform rect, RectTransform parent, float x, float y, float width, float height)
        {
            if (rect == null) return;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(width < 0 ? 1 : 0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            ConfigureReadableLayout();
            if (_content == null) return;
            var grid = _content.GetComponent<GridLayoutGroup>();
            var rect = _content as RectTransform;
            if (grid == null || rect == null) return;
            float width = rect.rect.width - grid.padding.horizontal;
            if (width <= 0) return;
            int columns = Mathf.Max(1, Mathf.FloorToInt((width + 20f) / 540f));
            var size = new Vector2((width - (columns - 1) * 20f) / columns, 320f);
            if (grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            if (grid.constraintCount != columns) grid.constraintCount = columns;
            if (grid.cellSize != size) grid.cellSize = size;
            if (grid.spacing != new Vector2(20, 20)) grid.spacing = new Vector2(20, 20);
        }

        private void Awake()
        {
            ConfigureScrollContent(_content);
            ConfigureReadableLayout();
            if (_statsButton != null)
                _statsButton.onClick.AddListener(ShowStats);

            if (_talentsButton != null)
                _talentsButton.onClick.AddListener(ShowTalents);

            ShowStats();
        }

        public static void ConfigureScrollContent(Transform content)
        {
            var rect = content as RectTransform;
            if (rect == null) return;
            var scroll = content.GetComponentInParent<ScrollRect>(true);
            if (scroll == null) return;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);
            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.enabled = true;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = rect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 25;
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
            if (_skillTreeView != null) _skillTreeView.CloseWindow();
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

                card.UseCollectionLayout();

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

        public void SelectMonster(MonsterInstance instance)
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
                if (_skillTreeView != null) _skillTreeView.CloseWindow();
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
            if (_showingTalents) RefreshTalents();
        }

        public void ShowStats()
        {
            if (_skillTreeView != null) _skillTreeView.CloseWindow();
            SetDetailsTab(false);
        }

        public void ShowTalents()
        {
            SetDetailsTab(true);
            RefreshTalents();
        }

        private void RefreshTalents()
        {
            if (_skillTreeView != null && _inventory != null)
            {
                if (_statsPanel != null && _talentsPanel != null)
                {
                    var stats = _statsPanel.transform as RectTransform;
                    var talents = _talentsPanel.transform as RectTransform;
                    if (stats != null && talents != null)
                    {
                        talents.SetParent(stats.parent, false);
                        talents.anchorMin = stats.anchorMin;
                        talents.anchorMax = stats.anchorMax;
                        talents.pivot = stats.pivot;
                        talents.anchoredPosition = stats.anchoredPosition;
                        talents.sizeDelta = stats.sizeDelta;
                        talents.localScale = stats.localScale;
                        _skillTreeView.UseEmbeddedLayout(talents);
                    }
                }
                _skillTreeView.OpenWindowForInstance(_selected, _inventory.GetDatabase());
            }
        }

        private void SetDetailsTab(bool talents)
        {
            _showingTalents = talents;
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
