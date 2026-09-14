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

        public void UseReadableLayout()
        {
            var rect = transform as RectTransform;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 120);
            foreach (var layout in GetComponentsInChildren<UnityEngine.UI.LayoutGroup>(true)) layout.enabled = false;
            PlaceReadable(_leftSlot, .18f, 112, 112);
            PlaceReadable(_rightSlot, .82f, 112, 112);
            PlaceReadable(_levelText, .5f, 180, 80);
            foreach (Transform child in transform)
                if ((_leftSlot == null || child != _leftSlot.transform) &&
                    (_rightSlot == null || child != _rightSlot.transform) &&
                    (_levelText == null || child != _levelText.transform)) child.gameObject.SetActive(false);
            if (_levelText != null)
            {
                _levelText.enableAutoSizing = false;
                _levelText.fontSize = 38;
                _levelText.alignment = TextAlignmentOptions.Center;
            }
        }

        private void PlaceReadable(Component component, float x, float width, float height)
        {
            if (component == null) return;
            var rect = component.transform as RectTransform;
            rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(x, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;
        }

        public void SetupRow(SkillNodeSO leftNode, SkillNodeSO rightNode, MonsterSkillTree tree, System.Action<SkillNodeSO, SkillNodeSO> onSelectCallback)
        {
            _leftNode = leftNode;
            _rightNode = rightNode;
            if (_leftSlot != null) _leftSlot.gameObject.SetActive(leftNode != null);
            if (_rightSlot != null) _rightSlot.gameObject.SetActive(rightNode != null);

            // 🎯 Captura e exibe o nível necessário da linha
            int requiredLevel = tree.GetRequiredLevel(leftNode != null ? leftNode : rightNode);
            bool levelLocked = tree.Progression.currentLevel < requiredLevel;

            if (_levelText != null)
            {
                _levelText.text = $"Lv. {requiredLevel}";
                _levelText.color = levelLocked ? Color.gray : Color.white;
            }

            // Configura o slot da esquerda
            if (_leftSlot != null && leftNode != null)
            {
                bool isUnlocked = tree.IsNodeUnlocked(leftNode.skillID);
                bool isBlocked = levelLocked || (rightNode != null && tree.IsNodeUnlocked(rightNode.skillID));
                bool canUnlock = tree.CanUnlockNodeInRow(leftNode, rightNode);

                _leftSlot.Setup(leftNode, tree, isUnlocked, isBlocked, canUnlock, () => onSelectCallback?.Invoke(leftNode, rightNode));
            }

            // Configura o slot da direita
            if (_rightSlot != null && rightNode != null)
            {
                bool isUnlocked = tree.IsNodeUnlocked(rightNode.skillID);
                bool isBlocked = levelLocked || (leftNode != null && tree.IsNodeUnlocked(leftNode.skillID));
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
