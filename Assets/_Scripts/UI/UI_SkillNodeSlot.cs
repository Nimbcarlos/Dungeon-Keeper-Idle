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
        [SerializeField] private Image _frameBorder;
        [SerializeField] private TextMeshProUGUI _levelRequirementText;

        [Header("Cores dos Estados")]
        [SerializeField] private Color _unlockedColor = Color.green;
        [SerializeField] private Color _availableColor = Color.yellow;
        [SerializeField] private Color _lockedColor = Color.gray;

        public SkillNodeSO NodeData { get; private set; }
        private MonsterSkillTree _currentTree;

        public void Setup(SkillNodeSO node, MonsterSkillTree tree, bool isUnlocked, bool canUnlock)
        {
            NodeData = node;
            _currentTree = tree;

            if (_iconImage != null && node.icon != null)
                _iconImage.sprite = node.icon;

            if (_levelRequirementText != null)
                _levelRequirementText.text = $"Lv.{node.requiredMonsterLevel}";

            // Atualiza o visual do frame baseado no estado
            if (isUnlocked)
            {
                _frameBorder.color = _unlockedColor;
                _nodeButton.interactable = true;
            }
            else if (canUnlock)
            {
                _frameBorder.color = _availableColor;
                _nodeButton.interactable = true;
            }
            else
            {
                _frameBorder.color = _lockedColor;
                _nodeButton.interactable = false;
            }

            _nodeButton.onClick.RemoveAllListeners();
            _nodeButton.onClick.AddListener(OnClickNode);
        }

        private void OnClickNode()
        {
            if (_currentTree == null || NodeData == null) return;

            // Tenta desbloquear o nó ao clicar
            if (_currentTree.TryUnlockNode(NodeData))
            {
                // O evento da árvore atualizará a UI completa
            }
        }
    }
}