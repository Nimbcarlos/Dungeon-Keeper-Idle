using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonKeeper
{
    public class UI_SkillNodeSlot : MonoBehaviour
    {
        [Header("Referências da UI")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _nodeButton;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _levelRequirementText;

        [Header("Cores dos Estados")]
        [SerializeField] private Color _unlockedColor = Color.green;
        [SerializeField] private Color _selectedColor = Color.cyan; // 🎯 Nova cor para o rascunho/selecionado
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _blockedColor = Color.gray;

        public SkillNodeSO NodeData { get; private set; }
        private MonsterSkillTree _currentTree;

        public void Setup(SkillNodeSO node, MonsterSkillTree tree, bool isUnlocked, bool isBlocked, bool isSelected, System.Action onClickAction)
        {
            NodeData = node;
            _currentTree = tree;

            if (node == null) return;

            // 1. Configura o Ícone
            if (_iconImage != null)
            {
                _iconImage.sprite = node.icon;
                _iconImage.enabled = node.icon != null;
                _iconImage.color = isBlocked ? new Color(0.4f, 0.4f, 0.4f, 0.5f) : Color.white;
            }

            // 2. Requisito de Nível
            if (_levelRequirementText != null)
            {
                _levelRequirementText.text = $"Lv.{node.requiredMonsterLevel}";
            }

            // 3. Botão
            if (_nodeButton != null)
            {
                _nodeButton.onClick.RemoveAllListeners();
                _nodeButton.onClick.AddListener(() => onClickAction?.Invoke());
                _nodeButton.interactable = true;
            }

            // 4. Hierarquia Visual do Frame (Prioridade: Desbloqueado > Bloqueado > Selecionado no Rascunho > Disponível)
            if (_frameImage != null)
            {
                if (isUnlocked)
                    _frameImage.color = _unlockedColor;
                else if (isBlocked)
                    _frameImage.color = _blockedColor;
                else if (isSelected)
                    _frameImage.color = _selectedColor; // 🎯 Aplica a cor do rascunho
                else
                    _frameImage.color = _availableColor;
            }
        }
    }
}
 
/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonKeeper
{
    public class UI_SkillNodeSlot : MonoBehaviour
    {
        [Header("Referências da UI")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _nodeButton;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _levelRequirementText;

        [Header("Cores dos Estados")]
        [SerializeField] private Color _unlockedColor = Color.green;
        [SerializeField] private Color _availableColor = Color.yellow;
        [SerializeField] private Color _lockedColor = Color.gray;

        public SkillNodeSO NodeData { get; private set; }
        private MonsterSkillTree _currentTree;


        public void Setup(SkillNodeSO node, MonsterSkillTree tree, bool isUnlocked, bool isBlocked, System.Action onClickAction)
        {
            NodeData = node;
            _currentTree = tree;

            if (node == null) return;

            // 🎯 Força a atribuição do sprite no componente Image do filho Icon
            if (_iconImage != null)
            {
                _iconImage.sprite = node.icon;
                _iconImage.enabled = node.icon != null;
                
                // Garante que a cor do ícone seja branca opaca para não ficar transparente
                // _iconImage.color = Color.white;
            }

            if (_levelRequirementText != null)
            {
                _levelRequirementText.text = $"Lv.{node.requiredMonsterLevel}";
            }

            if (_nodeButton != null)
            {
                _nodeButton.onClick.RemoveAllListeners();
                _nodeButton.onClick.AddListener(() => onClickAction?.Invoke());
                _nodeButton.interactable = !isUnlocked && !isBlocked;
            }

            // Feedback visual da borda do card
            if (_frameImage != null)
            {
                if (isUnlocked)
                    _frameImage.color = _unlockedColor;
                else if (isBlocked)
                    _frameImage.color = _lockedColor;
                else
                    _frameImage.color = _availableColor;
            }
        }

        public void OnClickNode()
        {
            if (_currentTree == null || NodeData == null) return;

            // Tenta desbloquear o nó no MonsterSkillTree
            if (_currentTree.TryUnlockNode(NodeData))
            {
                Debug.Log($"[UI_SkillNodeSlot] Nó {NodeData.skillName} desbloqueado com sucesso!");
            }
        }
    }
}
*/
