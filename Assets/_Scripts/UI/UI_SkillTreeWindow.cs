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
        private MonsterSkillTree _selectedSkillTree;
        private List<UI_SkillRowSlot> _instantiatedRows = new List<UI_SkillRowSlot>();

        // Estado do Rascunho (Draft)
        private SkillNodeSO _draftSelectedNode;
        private SkillNodeSO _draftOppositeNode;

        public bool IsOpen => _windowPanel != null && _windowPanel.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_acceptButton != null)
            {
                _acceptButton.onClick.AddListener(ConfirmDraftSelection);
            }

            CloseWindow();
        }

        public void OpenWindowForMonster(Monster monster)
        {
            if (monster == null) return;

            _selectedMonster = monster;
            _selectedSkillTree = monster.GetComponent<MonsterSkillTree>();

            if (_selectedSkillTree == null) return;

            _selectedSkillTree.OnSkillTreeUpdated -= RefreshUI;
            _selectedSkillTree.OnSkillTreeUpdated += RefreshUI;

            ClearDraft();
            _windowPanel.SetActive(true);
            RefreshUI();
        }

        public void CloseWindow()
        {
            if (_selectedSkillTree != null)
            {
                _selectedSkillTree.OnSkillTreeUpdated -= RefreshUI;
            }

            _selectedMonster = null;
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
            }
        }

        private void RefreshUI()
        {
            if (_selectedMonster == null || _selectedSkillTree == null) return;

            // 1. Atualiza dados do cabeçalho
            if (_monsterNameText != null) 
                _monsterNameText.text = _selectedMonster.Data != null ? _selectedMonster.Data.displayName : _selectedMonster.name;

            if (_monsterLevelText != null) 
                _monsterLevelText.text = $"Level {_selectedMonster.CurrentLevel}";

            if (_skillPointsText != null) 
                _skillPointsText.text = $"Available Points: {_selectedSkillTree.AvailableSkillPoints}";

            // 2. Limpa linhas antigas
            foreach (var row in _instantiatedRows)
            {
                if (row != null) Destroy(row.gameObject);
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
                    rowSlot.SetupRow(leftNode, rightNode, _selectedSkillTree, OnNodeClickedInDraft);
                    _instantiatedRows.Add(rowSlot);
                }
            }
        }
    }
}
