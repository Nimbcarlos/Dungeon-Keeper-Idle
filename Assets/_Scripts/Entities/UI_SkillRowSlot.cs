using UnityEngine;
using TMPro;

namespace DungeonKeeper
{
    public class UI_SkillRowSlot : MonoBehaviour
    {
        [Header("Referências dos Componentes")]
        [SerializeField] private UI_SkillNodeSlot _leftNodeSlot;
        [SerializeField] private UI_SkillNodeSlot _rightNodeSlot;
        [SerializeField] private TextMeshProUGUI _levelText;

        public void SetupRow(SkillNodeSO leftNode, SkillNodeSO rightNode, MonsterSkillTree tree)
        {
            // 1. Atualiza o badge central com o nível exigido da linha
            int reqLevel = leftNode != null ? leftNode.requiredMonsterLevel : (rightNode != null ? rightNode.requiredMonsterLevel : 1);
            Debug.Log("iniciou");
            
            if (_levelText != null)
                _levelText.text = $"LV. {reqLevel}";

            // 2. Configura o nó da esquerda
            if (_leftNodeSlot != null)
            {
                if (leftNode != null && tree != null)
                {
                    _leftNodeSlot.gameObject.SetActive(true);

                    bool isUnlocked = tree.IsNodeUnlocked(leftNode.skillID);
                    bool isBlocked = leftNode.mutuallyExclusiveSkill != null && tree.IsNodeUnlocked(leftNode.mutuallyExclusiveSkill.skillID);

                    _leftNodeSlot.Setup(leftNode, isUnlocked, isBlocked);
                }
                else
                {
                    _leftNodeSlot.gameObject.SetActive(false);
                }
            }

            // 3. Configura o nó da direita
            if (_rightNodeSlot != null)
            {
                if (rightNode != null && tree != null)
                {
                    _rightNodeSlot.gameObject.SetActive(true);

                    bool isUnlocked = tree.IsNodeUnlocked(rightNode.skillID);
                    bool isBlocked = rightNode.mutuallyExclusiveSkill != null && tree.IsNodeUnlocked(rightNode.mutuallyExclusiveSkill.skillID);

                    _rightNodeSlot.Setup(rightNode, isUnlocked, isBlocked);
                }
                else
                {
                    _rightNodeSlot.gameObject.SetActive(false);
                }
            }
        }
    }
}