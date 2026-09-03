using TMPro;
using System.Collections.Generic;
using UnityEngine;

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
            Debug.Log($"[SkillTreeWindow] 1. Tentando abrir para o monstro: {(monster != null ? monster.name : "NULL")}");
            if (monster == null) return;

            _selectedMonster = monster;
            _selectedSkillTree = monster.GetComponent<MonsterSkillTree>();

            if (_selectedSkillTree == null)
            {
                Debug.LogWarning($"[SkillTreeWindow] ERRO: O monstro {monster.name} NÃO possui o componente MonsterSkillTree!");
                return;
            }

            _selectedSkillTree.OnSkillTreeUpdated -= RefreshUI; // Evita duplicar inscrição
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
            Debug.Log("[SkillTreeWindow] 2. RefreshUI chamado.");

            if (_selectedMonster == null || _selectedSkillTree == null)
            {
                Debug.LogWarning($"[SkillTreeWindow] ERRO: _selectedMonster é {(_selectedMonster == null ? "NULL" : "OK")} ou _selectedSkillTree é {(_selectedSkillTree == null ? "NULL" : "OK")}");
                return;
            }

            // 1. Atualiza dados do cabeçalho
            if (_monsterNameText != null) 
                _monsterNameText.text = _selectedMonster.Data != null ? _selectedMonster.Data.displayName : _selectedMonster.name;

            if (_monsterLevelText != null) 
                _monsterLevelText.text = $"Level {_selectedMonster.CurrentLevel}";

            if (_skillPointsText != null) 
                _skillPointsText.text = $"Available Points: {_selectedSkillTree.AvailableSkillPoints}";

            // 2. Limpa linhas antigas da UI
            foreach (var row in _instantiatedRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _instantiatedRows.Clear();

            // 3. Checagem de segurança do Prefab e do Container
            if (_rowPrefab == null)
            {
                Debug.LogError("[SkillTreeWindow] ERRO CRÍTICO: '_rowPrefab' não foi arrastado no Inspector do UI_SkillTreeWindow!");
                return;
            }

            if (_nodesContainer == null)
            {
                Debug.LogError("[SkillTreeWindow] ERRO CRÍTICO: '_nodesContainer' não foi arrastado no Inspector do UI_SkillTreeWindow!");
                return;
            }

            // 4. Agrupa e popula os nós (compatível com IReadOnlyList)
            IReadOnlyList<SkillNodeSO> availableNodes = GetNodesFromTree(_selectedSkillTree);
            Debug.Log($"[SkillTreeWindow] 3. Total de nós encontrados na Tree: {(availableNodes != null ? availableNodes.Count : 0)}");

            if (availableNodes == null || availableNodes.Count == 0)
            {
                Debug.LogWarning("[SkillTreeWindow] A lista AvailableNodes da MonsterSkillTree está VAZIA! Adicione os SO_SkillNode no Inspector do Monstro/Data.");
                return;
            }

            for (int i = 0; i < availableNodes.Count; i += 2)
            {
                SkillNodeSO leftNode = availableNodes[i];
                SkillNodeSO rightNode = (i + 1 < availableNodes.Count) ? availableNodes[i + 1] : null;

                Debug.Log($"[SkillTreeWindow] 4. Processando Linha {i / 2}: Esquerda = {(leftNode != null ? leftNode.skillName : "NULL")}, Direita = {(rightNode != null ? rightNode.skillName : "NULL")}");

                if (leftNode == null && rightNode == null) continue;

                // Instancia o prefab da LINHA
                GameObject rowObj = Instantiate(_rowPrefab, _nodesContainer);
                UI_SkillRowSlot rowSlot = rowObj.GetComponent<UI_SkillRowSlot>();

                if (rowSlot != null)
                {
                    Debug.Log($"[SkillTreeWindow] 5. UI_SkillRowSlot encontrado no Prefab! Chamando SetupRow...");
                    rowSlot.SetupRow(leftNode, rightNode, _selectedSkillTree);
                    _instantiatedRows.Add(rowSlot);
                }
                else
                {
                    Debug.LogError($"[SkillTreeWindow] ERRO CRÍTICO: O Prefab '{_rowPrefab.name}' foi instanciado, mas NÃO tem o componente 'UI_SkillRowSlot' anexado na raiz dele!");
                }
            }
        }

        // 🎯 FIX: O tipo de retorno foi alterado para IReadOnlyList para casar com o MonsterSkillTree.cs
        private IReadOnlyList<SkillNodeSO> GetNodesFromTree(MonsterSkillTree tree)
        {
            if (tree == null) return System.Array.Empty<SkillNodeSO>();
            return tree.AvailableNodes;
        }
    }
}