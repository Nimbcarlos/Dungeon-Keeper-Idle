using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonKeeper
{
    public class UI_MonsterListItem : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Button _itemButton;
        [SerializeField] private GameObject _equippedTag; // Imagem/Texto opcional "EM USO"

        private MonsterData _data;

        public void Setup(MonsterData data, bool isEquipped, System.Action<MonsterData> onClickCallback)
        {
            _data = data;

            if (_iconImage != null) _iconImage.sprite = data.icon;
            if (_nameText != null) _nameText.text = data.displayName;

            // Se o monstro já está em uso, bloqueia a interação do botão
            if (_itemButton != null)
            {
                _itemButton.interactable = !isEquipped;
                _itemButton.onClick.RemoveAllListeners();
                _itemButton.onClick.AddListener(() => onClickCallback?.Invoke(_data));
            }

            if (_equippedTag != null)
            {
                _equippedTag.SetActive(isEquipped);
            }
        }
    }
}