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

        public void Setup(SkillNodeSO node, bool isUnlocked, bool isBlockedByOpposite)
        {
            _iconImage.sprite = node.icon;

            if (isUnlocked)
            {
                // ✅ Já Comprado: Fica bem visível / Borda Dourada ou Verde
                _frameImage.color = Color.green;
                _iconImage.color = Color.white;
            }
            else if (isBlockedByOpposite)
            {
                // ❌ Descartado (Escolheu a outra opção): Escuro / Cinza Transparente
                _frameImage.color = Color.gray;
                _iconImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Opaco / Desativado
            }
            else
            {
                // 🔓 Disponível para compra: Cor Normal
                _frameImage.color = Color.white;
                _iconImage.color = Color.white;
            }
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