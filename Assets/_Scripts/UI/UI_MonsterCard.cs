using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonKeeper
{
    public class UI_MonsterCard : MonoBehaviour
    {
        [Header("Referências de UI")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Button _cardButton;

        private MonsterData _data;
        private System.Action<MonsterData> _onSelectedCallback;

        public void Setup(MonsterData data, int availableCount, System.Action<MonsterData> onSelected)
        {
            _data = data;
            _onSelectedCallback = onSelected;

            if (_iconImage != null && data != null && data.icon != null) 
                _iconImage.sprite = data.icon;

            if (_nameText != null && data != null) 
            {
                _nameText.text = availableCount > 1 ? $"{data.displayName} x{availableCount}" : data.displayName;
            }

            if (_cardButton != null)
            {
                _cardButton.onClick.RemoveAllListeners();
                _cardButton.onClick.AddListener(() => _onSelectedCallback?.Invoke(_data));
            }
        }
    }
}