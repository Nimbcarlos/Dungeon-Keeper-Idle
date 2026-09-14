using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DungeonKeeper
{
    public class UI_SummoningItemCard : MonoBehaviour
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text detailText;
        public GameObject selection;
        public Button button;
        private SummoningItemData _item;
        private Action<SummoningItemData> _callback;
        private void Awake() { button.onClick.AddListener(Click); }
        private void OnDestroy() { if (button != null) button.onClick.RemoveListener(Click); }
        private void Click() => _callback?.Invoke(_item);
        public void Setup(SummoningItemData item, Action<SummoningItemData> callback)
        { _item = item; _callback = callback; icon.sprite = item.icon; icon.preserveAspect = true; nameText.text = item.displayName; }
        public void Refresh(bool selected)
        {
            var count = SummoningItemInventory.Instance?.GetQuantity(_item) ?? 0;
            detailText.text = _item.rarity + "  •  Owned: " + count + "\n" + UI_SummoningPage.FormatTime(_item.summonDurationSeconds);
            selection.SetActive(selected);
            button.interactable = true;
        }
    }
}