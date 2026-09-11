using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonKeeper
{
    public class UI_SummoningShopCard : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _durationText;
        [SerializeField] private TextMeshProUGUI _ownedText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private Button _buyButton;

        [Header("Rotulos")]
        [SerializeField] private string _durationLabel = "Summoning time";
        [SerializeField] private string _ownedLabel = "Owned";
        [SerializeField] private string _quantityLabel = "Quantity";

        private SummoningItemData _item;
        private Action<SummoningItemData> _onBuy;

        public void Setup(SummoningItemData item, Action<SummoningItemData> onBuy)
        {
            _item = item;
            _onBuy = onBuy;
            if (_buyButton != null)
            {
                _buyButton.onClick.RemoveListener(Buy);
                _buyButton.onClick.AddListener(Buy);
            }
            if (_icon != null)
            {
                _icon.sprite = item != null ? item.icon : null;
                _icon.enabled = _icon.sprite != null;
                _icon.preserveAspect = true;
            }
            if (_nameText != null) _nameText.text = item != null ? item.displayName : "";
            if (_rarityText != null) _rarityText.text = item != null ? item.rarity.ToString() : "";
            if (_descriptionText != null) _descriptionText.text = item != null ? item.description : "";
            if (_priceText != null) _priceText.text = item != null ? item.essencePrice.ToString() : "";
            if (_quantityText != null) _quantityText.text = item != null ? $"{_quantityLabel}: {item.quantityPerPurchase}" : "";
            if (_durationText != null)
            {
                int seconds = item != null ? Mathf.Max(0, item.summonDurationSeconds) : 0;
                _durationText.text = $"{_durationLabel}: {seconds / 3600:00}:{seconds / 60 % 60:00}:{seconds % 60:00}";
            }
        }

        public void RefreshState(EssenceShopManager shop, SummoningItemInventory inventory)
        {
            if (_ownedText != null)
                _ownedText.text = $"{_ownedLabel}: {(inventory != null ? inventory.GetQuantity(_item) : 0)}";
            if (_buyButton != null)
                _buyButton.interactable = _item != null && shop != null && shop.CanBuy(_item);
        }

        private void Buy() { if (_item != null) _onBuy?.Invoke(_item); }
        private void OnDestroy() { if (_buyButton != null) _buyButton.onClick.RemoveListener(Buy); }
    }
}
