using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class UI_SkillTreeWindow : MonoBehaviour
    {
        public static UI_SkillTreeWindow Instance { get; private set; }

        [Header("Painel Principal")]
        [SerializeField] private GameObject _windowPanel;

        [Header("Textos de Detalhes")]
        [SerializeField] private TextMeshProUGUI _monsterNameText;
        [SerializeField] private TextMeshProUGUI _skillPointsText;
        [SerializeField] private TextMeshProUGUI _monsterLevelText;

        [Header("Container dos Nós")]
        [SerializeField] private Transform _nodesContainer;
        [SerializeField] private GameObject _rowPrefab; 

        [Header("Painel de Rascunho & Descrição (Separados)")]
        [SerializeField] private TextMeshProUGUI _flavorText;      // 📜 Texto de citação/anime
        [SerializeField] private TextMeshProUGUI _descriptionText; // ⚔️ Descrição técnica do bônus
        [SerializeField] private Button _acceptButton;

        [Header("Botões de Ação")]
        [SerializeField] private GameObject _closeButton;

        private Monster _selectedMonster;
        private MonsterData _selectedData;
        private MonsterSkillTree _inventoryTree;
        private MonsterSkillTree _selectedSkillTree;
        private bool _initialized;
        private bool _embedded;
        private int _displayedLevel;
        private List<UI_SkillRowSlot> _instantiatedRows = new List<UI_SkillRowSlot>();

        // Estado do Rascunho (Draft)
        private SkillNodeSO _draftSelectedNode;
        private SkillNodeSO _draftOppositeNode;

        public bool IsOpen => _windowPanel != null && _windowPanel.activeSelf;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_acceptButton != null)
            {
                _acceptButton.onClick.AddListener(ConfirmDraftSelection);
            }

            CloseWindow();
        }

        public void UseEmbeddedLayout(RectTransform host)
        {
            if (_embedded || host == null || _windowPanel == null) return;
            EnsureInitialized();
            _embedded = true;
            var root = _windowPanel.transform as RectTransform;
            root.SetParent(host, false);
            Place(root, host, Vector2.zero, Vector2.one);
            foreach (Transform child in host)
                if (child != root) child.gameObject.SetActive(false);
            // Hide the old floating-window chrome after moving its useful controls.
            var oldChildren = new List<GameObject>();
            foreach (Transform child in root) oldChildren.Add(child.gameObject);
            var viewport = new GameObject("TalentViewport", typeof(RectTransform), typeof(Image),
                typeof(RectMask2D), typeof(ScrollRect));
            var viewRect = viewport.GetComponent<RectTransform>();
            Place(viewRect, root, new Vector2(0, .44f), new Vector2(1, .88f));
            viewport.GetComponent<Image>().color = new Color(0, 0, 0, .08f);
            var content = _nodesContainer as RectTransform;
            if (content != null)
            {
                foreach (var oldLayout in content.GetComponents<LayoutGroup>()) oldLayout.enabled = false;
                Place(content, viewRect, new Vector2(0, 1), Vector2.one);
                content.pivot = new Vector2(.5f, 1);
                var layout = content.GetComponent<VerticalLayoutGroup>();
                if (layout == null) layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.enabled = true;
                layout.spacing = 16;
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                var fitter = content.GetComponent<ContentSizeFitter>();
                if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var scroll = viewport.GetComponent<ScrollRect>();
                scroll.viewport = viewRect;
                scroll.content = content;
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.scrollSensitivity = 25;
            }
            if (_skillPointsText != null)
            {
                Place(_skillPointsText.rectTransform, root, new Vector2(0, .88f), Vector2.one);
                _skillPointsText.fontSize = 36;
                _skillPointsText.enableAutoSizing = false;
                _skillPointsText.alignment = TextAlignmentOptions.MidlineLeft;
            }
            if (_descriptionText != null)
            {
                var descriptionView = new GameObject("TalentDescriptionViewport", typeof(RectTransform),
                    typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
                var descriptionRect = descriptionView.GetComponent<RectTransform>();
                Place(descriptionRect, root, Vector2.zero, new Vector2(1, .42f));
                descriptionRect.offsetMin = new Vector2(0, 116);
                descriptionView.GetComponent<Image>().color = new Color(0, 0, 0, .08f);
                Place(_descriptionText.rectTransform, descriptionRect, new Vector2(0, 1), Vector2.one);
                _descriptionText.rectTransform.pivot = new Vector2(.5f, 1);
                _descriptionText.fontSize = 36;
                _descriptionText.enableAutoSizing = false;
                _descriptionText.textWrappingMode = TextWrappingModes.Normal;
                _descriptionText.overflowMode = TextOverflowModes.Overflow;
                var descriptionFit = _descriptionText.GetComponent<ContentSizeFitter>();
                if (descriptionFit == null) descriptionFit = _descriptionText.gameObject.AddComponent<ContentSizeFitter>();
                descriptionFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                descriptionFit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                var descriptionScroll = descriptionView.GetComponent<ScrollRect>();
                descriptionScroll.viewport = descriptionRect;
                descriptionScroll.content = _descriptionText.rectTransform;
                descriptionScroll.horizontal = false;
                descriptionScroll.movementType = ScrollRect.MovementType.Clamped;
                descriptionScroll.scrollSensitivity = 25;
            }
            if (_acceptButton != null)
            {
                Place(_acceptButton.transform as RectTransform, root, Vector2.zero, new Vector2(1, 0));
                var buttonRect = _acceptButton.transform as RectTransform;
                buttonRect.pivot = new Vector2(.5f, 0);
                buttonRect.sizeDelta = new Vector2(0, 108);
                foreach (var label in _acceptButton.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    label.fontSize = 38;
                    label.enableAutoSizing = false;
                }
            }
            foreach (var child in oldChildren)
                if (child != null && child.transform.parent == root &&
                    child.transform != _nodesContainer &&
                    (_skillPointsText == null || child != _skillPointsText.gameObject) &&
                    (_descriptionText == null || child != _descriptionText.gameObject) &&
                    (_acceptButton == null || child != _acceptButton.gameObject)) child.SetActive(false);
            if (_closeButton != null) _closeButton.SetActive(false);
            var canvas = root.GetComponent<Canvas>();
            if (canvas != null) canvas.overrideSorting = false;
        }

        private static void Place(RectTransform rect, RectTransform parent, Vector2 min, Vector2 max)
        {
            if (rect == null) return;
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_selectedSkillTree != null && _displayedLevel != _selectedSkillTree.Progression.currentLevel)
            {
                ClearDraft();
                RefreshUI();
            }
        }

        public void OpenWindowForMonster(Monster monster)
        {
            if (monster == null) return;
            EnsureInitialized();
            CloseWindow();

            _selectedMonster = monster;
            _selectedData = monster.Data;
            _selectedSkillTree = monster.GetComponent<MonsterSkillTree>();

            if (_selectedSkillTree == null) return;

            _selectedSkillTree.OnSkillTreeUpdated -= RefreshUI;
            _selectedSkillTree.OnSkillTreeUpdated += RefreshUI;

            ClearDraft();
            _windowPanel.SetActive(true);
            RefreshUI();
        }

        public void OpenWindowForInstance(MonsterInstance instance, MonsterDatabase database)
        {
            EnsureInitialized();
            CloseWindow();
            if (instance == null) return;
            _selectedData = instance.GetData(database);
            if (_selectedData == null) return;
            instance.progression ??= new MonsterProgression();
            if (_inventoryTree == null)
                _inventoryTree = gameObject.AddComponent<MonsterSkillTree>();
            _inventoryTree.InitializeTree(_selectedData, instance.progression);
            _selectedSkillTree = _inventoryTree;
            _selectedSkillTree.OnSkillTreeUpdated += RefreshUI;
            if (_windowPanel != null) _windowPanel.SetActive(true);
            RefreshUI();
        }

        private void OnDisable()
        {
            if (_selectedSkillTree != null)
                _selectedSkillTree.OnSkillTreeUpdated -= RefreshUI;
        }

        public void CloseWindow()
        {
            if (_selectedSkillTree != null)
            {
                _selectedSkillTree.OnSkillTreeUpdated -= RefreshUI;
            }

            _selectedMonster = null;
            _selectedData = null;
            _selectedSkillTree = null;
            ClearDraft();

            if (_windowPanel != null)
                _windowPanel.SetActive(false);
        }

        private void ClearDraft()
        {
            _draftSelectedNode = null;
            _draftOppositeNode = null;

            if (_flavorText != null)
                _flavorText.text = string.Empty;

            if (_descriptionText != null)
                _descriptionText.text = "Select a skill node to preview its effects.";

            if (_acceptButton != null)
                _acceptButton.interactable = false;
        }

        private void OnNodeClickedInDraft(SkillNodeSO clickedNode, SkillNodeSO oppositeNode)
        {
            if (_selectedSkillTree == null || clickedNode == null) return;

            // 1. Atualiza o Flavor Text (Citação)
            if (_flavorText != null)
            {
                _flavorText.text = !string.IsNullOrEmpty(clickedNode.flavorText) 
                    ? $"\"{clickedNode.flavorText}\"" 
                    : string.Empty;
            }

            // 2. Se o nó já estiver comprado/desbloqueado
            if (_selectedSkillTree.IsNodeUnlocked(clickedNode.skillID))
            {
                _draftSelectedNode = null;
                _draftOppositeNode = null;

                if (_descriptionText != null)
                {
                    _descriptionText.text = $"<b>{clickedNode.skillName}</b>\n<color=green>[Unlocked]</color> {clickedNode.description}";
                }

                if (_acceptButton != null) _acceptButton.interactable = false;
                return;
            }

            // 3. Seleção Provisória (Draft)
            _draftSelectedNode = clickedNode;
            _draftOppositeNode = oppositeNode;

            if (_descriptionText != null)
            {
                _descriptionText.text = $"{clickedNode.description}";
            }

            // 4. Valida se o monstro pode comprar este nó para liberar o botão Accept
            bool canUnlock = _selectedSkillTree.CanUnlockNodeInRow(clickedNode, oppositeNode);
            if (_acceptButton != null)
            {
                _acceptButton.interactable = canUnlock;
            }
        }

        private void ConfirmDraftSelection()
        {
            if (_selectedSkillTree == null || _draftSelectedNode == null) return;

            // Tenta efetivar a compra no sistema
            if (_selectedSkillTree.TryUnlockNode(_draftSelectedNode, _draftOppositeNode))
            {
                Debug.Log($"[SkillTreeWindow] Seleção confirmada para: {_draftSelectedNode.skillName}");
                ClearDraft();
                RefreshUI();
                // A collection choice also updates the equipped copy sharing this progression.
                foreach (var tree in FindObjectsByType<MonsterSkillTree>(FindObjectsInactive.Exclude))
                    if (tree != _selectedSkillTree && tree.Progression == _selectedSkillTree.Progression)
                        tree.ApplySkillModifiers();
                SaveManager.Instance?.SaveGame();
            }
        }

        private void RefreshUI()
        {
            if (_selectedData == null || _selectedSkillTree == null) return;
            _displayedLevel = _selectedSkillTree.Progression.currentLevel;

            // 1. Atualiza dados do cabeçalho
            if (_monsterNameText != null) 
                _monsterNameText.text = _selectedData.displayName;

            if (_monsterLevelText != null) 
                _monsterLevelText.text = $"Level {_selectedSkillTree.Progression.currentLevel}";

            if (_skillPointsText != null) 
                _skillPointsText.text = $"Available Points: {_selectedSkillTree.AvailableSkillPoints}";

            // 2. Limpa linhas antigas
            foreach (var row in _instantiatedRows)
            {
                if (row != null)
                {
                    row.gameObject.SetActive(false);
                    Destroy(row.gameObject);
                }
            }
            _instantiatedRows.Clear();

            if (_rowPrefab == null || _nodesContainer == null) return;

            // 3. Popula as linhas
            IReadOnlyList<SkillNodeSO> availableNodes = _selectedSkillTree.AvailableNodes;
            if (availableNodes == null || availableNodes.Count == 0) return;

            for (int i = 0; i < availableNodes.Count; i += 2)
            {
                SkillNodeSO leftNode = availableNodes[i];
                SkillNodeSO rightNode = (i + 1 < availableNodes.Count) ? availableNodes[i + 1] : null;

                if (leftNode == null && rightNode == null) continue;

                GameObject rowObj = Instantiate(_rowPrefab, _nodesContainer);
                UI_SkillRowSlot rowSlot = rowObj.GetComponent<UI_SkillRowSlot>();

                if (rowSlot != null)
                {
                    if (_embedded) rowSlot.UseReadableLayout();
                    rowSlot.SetupRow(leftNode, rightNode, _selectedSkillTree, OnNodeClickedInDraft);
                    _instantiatedRows.Add(rowSlot);
                }
            }
        }
    }
}
