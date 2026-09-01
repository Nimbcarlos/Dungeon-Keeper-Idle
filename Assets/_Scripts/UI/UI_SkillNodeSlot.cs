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
                Debug.Log($"_iconImage: {(_iconImage != null ? "Encontrado" : "Não encontrado")}, node.icon: {(node.icon != null ? node.icon.name : "null")}");
                _iconImage.sprite = node.icon;
                Debug.Log($"_iconImage.sprite: {(_iconImage.sprite != null ? _iconImage.sprite.name : "null")}");
                _iconImage.enabled = node.icon != null;
                Debug.Log($"[UI_SkillNodeSlot] Configurando nó {node.skillName} com ícone {node.icon}.");
                
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
            Debug.Log($"[UI_SkillNodeSlot] Clique no nó {NodeData?.skillName ?? "null"}.");
            if (_currentTree == null || NodeData == null) return;

            // Tenta desbloquear o nó no MonsterSkillTree
            if (_currentTree.TryUnlockNode(NodeData))
            {
                Debug.Log($"[UI_SkillNodeSlot] Nó {NodeData.skillName} desbloqueado com sucesso!");
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
*/