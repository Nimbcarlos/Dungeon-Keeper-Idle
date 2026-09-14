using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DungeonKeeper
{
    public class UI_SummoningSlot : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text statusText;
        public Image icon;
        public Slider progress;
        public Button actionButton;
        public TMP_Text actionText;
        private int _index;
        private Action<int> _callback;
        private void Awake() { actionButton.onClick.AddListener(Click); }
        private void OnDestroy() { if(actionButton != null) actionButton.onClick.RemoveListener(Click); }
        private void Click() => _callback?.Invoke(_index);
        public void Setup(int index, Action<int> callback) { _index = index; _callback = callback; }
        public void Refresh(SummoningManager manager)
        {
            var state = manager.GetSlot(_index);
            var item = state == null ? null : manager.FindItem(state.itemID);
            titleText.text = state == null ? "Summoning altar " + (_index + 1) : item != null ? item.displayName : "Summoning";
            icon.sprite = item == null ? null : item.icon;
            icon.enabled = item != null && item.icon != null;
            double remaining = manager.Remaining(_index);
            bool ready = state != null && remaining <= 0;
            statusText.text = state == null ? "Empty slot\nSelect an item to begin." :
                ready ? "Ready to summon!" : "Preparing\n" + UI_SummoningPage.FormatTime(remaining);
            progress.gameObject.SetActive(state != null);
            progress.SetValueWithoutNotify(state == null ? 0 : ready ? 1 : (float)(1 - remaining / Math.Max(1, state.completesUtc - state.startedUtc)));
            actionText.text = state == null ? "Select item" : ready ? "Summon monster" : "Preparing...";
            actionButton.interactable = state == null || ready;
        }
    }
}