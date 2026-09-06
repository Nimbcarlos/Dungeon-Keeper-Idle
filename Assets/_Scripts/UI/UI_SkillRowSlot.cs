using UnityEngine;
using TMPro;

namespace DungeonKeeper
{
    public class UI_SkillRowSlot : MonoBehaviour
    {
        [Header("Slots dos Nós")]
        [SerializeField] private UI_SkillNodeSlot _leftSlot;
        [SerializeField] private UI_SkillNodeSlot _rightSlot;

        [Header("Badge Central de Nível")]
        [SerializeField] private TextMeshProUGUI _levelText;

        private SkillNodeSO _leftNode;
        private SkillNodeSO _rightNode;

        public void SetupRow(SkillNodeSO leftNode, SkillNodeSO rightNode, MonsterSkillTree tree, System.Action<SkillNodeSO, SkillNodeSO> onSelectCallback)
        {
            _leftNode = leftNode;
            _rightNode = rightNode;

            // 🎯 Captura e exibe o nível necessário da linha
            int requiredLevel = 1;
            if (leftNode != null) requiredLevel = leftNode.requiredMonsterLevel;
            else if (rightNode != null) requiredLevel = rightNode.requiredMonsterLevel;

            if (_levelText != null)
            {
                _levelText.text = $"Lv. {requiredLevel}";
            }

            // Configura o slot da esquerda
            if (_leftSlot != null && leftNode != null)
            {
                bool isUnlocked = tree.IsNodeUnlocked(leftNode.skillID);
                bool isBlocked = rightNode != null && tree.IsNodeUnlocked(rightNode.skillID);
                bool canUnlock = tree.CanUnlockNodeInRow(leftNode, rightNode);

                _leftSlot.Setup(leftNode, tree, isUnlocked, isBlocked, canUnlock, () => onSelectCallback?.Invoke(leftNode, rightNode));
            }

            // Configura o slot da direita
            if (_rightSlot != null && rightNode != null)
            {
                bool isUnlocked = tree.IsNodeUnlocked(rightNode.skillID);
                bool isBlocked = leftNode != null && tree.IsNodeUnlocked(leftNode.skillID);
                bool canUnlock = tree.CanUnlockNodeInRow(rightNode, leftNode);

                _rightSlot.Setup(rightNode, tree, isUnlocked, isBlocked, canUnlock, () => onSelectCallback?.Invoke(rightNode, leftNode));
            }
        }
    }
}

/*using UnityEngine;

namespace DungeonKeeper
{
    public class UI_SkillRowSlot : MonoBehaviour
    {
        [SerializeField] private UI_SkillNodeSlot _leftSlot;
        [SerializeField] private UI_SkillNodeSlot _rightSlot;

        private SkillNodeSO _leftNode;
        private SkillNodeSO _rightNode;
        private MonsterSkillTree _tree;

        public void SetupRow(SkillNodeSO leftNode, SkillNodeSO rightNode, MonsterSkillTree tree)
        {
            _leftNode = leftNode;
            _rightNode = rightNode;
            _tree = tree;

            // 1. Configura o slot da esquerda (passando o da direita como oposto)
            if (_leftSlot != null && leftNode != null)
            {
                bool canUnlock = tree.CanUnlockNodeInRow(leftNode, rightNode);
                bool isUnlocked = tree.IsNodeUnlocked(leftNode.skillID);
                bool isBlocked = rightNode != null && tree.IsNodeUnlocked(rightNode.skillID);

                _leftSlot.Setup(leftNode, tree, isUnlocked, isBlocked, () => OnClickSlot(leftNode, rightNode));
            }

            // 2. Configura o slot da direita (passando o da esquerda como oposto)
            if (_rightSlot != null && rightNode != null)
            {
                bool canUnlock = tree.CanUnlockNodeInRow(rightNode, leftNode);
                bool isUnlocked = tree.IsNodeUnlocked(rightNode.skillID);
                bool isBlocked = leftNode != null && tree.IsNodeUnlocked(leftNode.skillID);

                _rightSlot.Setup(rightNode, tree, isUnlocked, isBlocked, () => OnClickSlot(rightNode, leftNode));
            }
        }

        private void OnClickSlot(SkillNodeSO clickedNode, SkillNodeSO oppositeNode)
        {
            if (_tree == null || clickedNode == null) return;

            // Tenta desbloquear passando o nó rival para travar a linha
            _tree.TryUnlockNode(clickedNode, oppositeNode);
        }
    }
}
*/