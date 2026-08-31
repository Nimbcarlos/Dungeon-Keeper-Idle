using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        [SerializeField] private GameObject _rowPrefab; // 🎯 AGORA APONTA PARA O PREFAB DA LINHA (UI_SkillRowSlot)

        [Header("Botões de Ação")]
        [SerializeField] private GameObject _closeButton;

        private Monster _selectedMonster;
        private MonsterSkillTree _selectedSkillTree;
        private List<UI_SkillRowSlot> _instantiatedRows = new List<UI_SkillRowSlot>();

        public bool IsOpen => _windowPanel != null && _windowPanel.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            CloseWindow();
        }

        public void OpenWindowForMonster(Monster monster)
        {
            if (monster == null) return;

            _selectedMonster = monster;
            _selectedSkillTree = monster.GetComponent<MonsterSkillTree>();

            if (_selectedSkillTree == null)
            {
                Debug.LogWarning($"O monstro {monster.name} não possui o componente MonsterSkillTree!");
                return;
            }

            _selectedSkillTree.OnSkillTreeUpdated += RefreshUI;

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

            if (_windowPanel != null)
                _windowPanel.SetActive(false);
        }

        private void RefreshUI()
        {
            if (_selectedMonster == null || _selectedSkillTree == null) return;

            // 1. Atualiza dados do cabeçalho
            if (_monsterNameText != null) 
                _monsterNameText.text = _selectedMonster.Data != null ? _selectedMonster.Data.displayName : _selectedMonster.name;

            if (_monsterLevelText != null) 
                _monsterLevelText.text = $"Nível {_selectedMonster.CurrentLevel}";

            if (_skillPointsText != null) 
                _skillPointsText.text = $"Pontos Disponíveis: {_selectedSkillTree.AvailableSkillPoints}";

            // 2. Limpa linhas antigas da UI
            foreach (var row in _instantiatedRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _instantiatedRows.Clear();

            // 3. Agrupa e popula os nós em pares/linhas
            List<SkillNodeSO> availableNodes = GetNodesFromTree(_selectedSkillTree);

            for (int i = 0; i < availableNodes.Count; i += 2)
            {
                SkillNodeSO leftNode = availableNodes[i];
                SkillNodeSO rightNode = (i + 1 < availableNodes.Count) ? availableNodes[i + 1] : null;

                if (leftNode == null && rightNode == null) continue;

                // Instancia o prefab da LINHA
                GameObject rowObj = Instantiate(_rowPrefab, _nodesContainer);
                UI_SkillRowSlot rowSlot = rowObj.GetComponent<UI_SkillRowSlot>();

                if (rowSlot != null)
                {
                    rowSlot.SetupRow(leftNode, rightNode, _selectedSkillTree);
                    _instantiatedRows.Add(rowSlot);
                }
            }
        }

        private List<SkillNodeSO> GetNodesFromTree(MonsterSkillTree tree)
        {
            if (tree == null) return new List<SkillNodeSO>();
            return tree.AvailableNodes;
        }
    }
}