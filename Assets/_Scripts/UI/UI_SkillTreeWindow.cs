using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private GameObject _nodePrefab;

        [Header("Botões de Ação")]
        [SerializeField] private Button _closeButton;

        private Monster _selectedMonster;
        private MonsterSkillTree _selectedSkillTree;
        private List<UI_SkillNodeSlot> _instantiatedSlots = new List<UI_SkillNodeSlot>();

        public bool IsOpen => _windowPanel != null && _windowPanel.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseWindow);

            CloseWindow();
        }

        public void OpenWindowForMonster(Monster monster)
        {
            Debug.Log($"Abrindo janela de Skill Tree para o monstro: {monster.name}");
            if (monster == null) return;

            _selectedMonster = monster;
            _selectedSkillTree = monster.GetComponent<MonsterSkillTree>();

            if (_selectedSkillTree == null)
            {
                Debug.LogWarning($"O monstro {monster.name} não possui o componente MonsterSkillTree!");
                return;
            }

            // Inscreve no evento de atualização da árvore do monstro
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

            // 1. Atualiza cabeçalho com dados do monstro
            if (_monsterNameText != null) 
                _monsterNameText.text = _selectedMonster.Data != null ? _selectedMonster.Data.displayName : _selectedMonster.name;

            if (_monsterLevelText != null) 
                _monsterLevelText.text = $"Nível {_selectedMonster.CurrentLevel}";

            if (_skillPointsText != null) 
                _skillPointsText.text = $"Pontos Disponíveis: {_selectedSkillTree.AvailableSkillPoints}";

            // 2. Limpa slots antigos da UI
            foreach (var slot in _instantiatedSlots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _instantiatedSlots.Clear();

            // 3. Popula os nós configurados na SkillTree
            // Busca os nós disponíveis do componente MonsterSkillTree
            List<SkillNodeSO> availableNodes = GetNodesFromTree(_selectedSkillTree);
            List<string> unlockedIDs = _selectedSkillTree.GetUnlockedSkillIDs();

            foreach (var node in availableNodes)
            {
                if (node == null) continue;

                GameObject obj = Instantiate(_nodePrefab, _nodesContainer);
                UI_SkillNodeSlot slot = obj.GetComponent<UI_SkillNodeSlot>();

                if (slot != null)
                {
                    bool isUnlocked = unlockedIDs.Contains(node.skillID);
                    bool canUnlock = _selectedSkillTree.CanUnlockNode(node);

                    slot.Setup(node, _selectedSkillTree, isUnlocked, canUnlock);
                    _instantiatedSlots.Add(slot);
                }
            }
        }

        private List<SkillNodeSO> GetNodesFromTree(MonsterSkillTree tree)
        {
            if (tree == null) return new List<SkillNodeSO>();
            // Método auxiliar para recuperar os nós do script
            // Caso tenha tornado o _availableNodes público ou privado com getter
            return tree.AvailableNodes;
        }
    }
}