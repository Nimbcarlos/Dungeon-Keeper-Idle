using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DungeonKeeper
{
    public class UI_SummoningPage : MonoBehaviour
    {
        public Transform content;
        public UI_SummoningItemCard cardPrefab;
        public Transform slotsContainer;
        public UI_SummoningSlot slotPrefab;
        public Image itemIcon;
        public TMP_Text nameText, descriptionText, durationText, possibleMonstersText, feedbackText, emptyText;
        public Button useButton, shopButton;
        public UI_CollectionWindow collection;
        public UI_EssenceShopWindow shop;
        public UI_SummonResultWindow resultWindow;
        private SummoningManager _manager;
        private SummoningItemInventory _inventory;
        private SummoningItemData _selected;
        private readonly List<UI_SummoningItemCard> _cards = new();
        private readonly List<SummoningItemData> _items = new();
        private readonly List<UI_SummoningSlot> _slots = new();
        private bool _dirty;
        private int _targetSlot = -1;
        private void Awake()
        {
            useButton.onClick.AddListener(Begin);
            shopButton.onClick.AddListener(OpenShop);
        }
        private void OnEnable() { Connect(); }
        private void Start() { if (_manager == null) Connect(); }
        private void Connect()
        {
            Disconnect();
            _manager = SummoningManager.Instance;
            _inventory = SummoningItemInventory.Instance;
            if (_manager != null) _manager.Changed += MarkDirty;
            if (_inventory != null) _inventory.OnInventoryChanged += MarkDirty;
            Rebuild();
        }
        private void Disconnect()
        {
            if (_manager != null) _manager.Changed -= MarkDirty;
            if (_inventory != null) _inventory.OnInventoryChanged -= MarkDirty;
        }
        private void OnDisable() { Disconnect(); }
        private void OnDestroy()
        {
            Disconnect();
            if (useButton != null) useButton.onClick.RemoveListener(Begin);
            if (shopButton != null) shopButton.onClick.RemoveListener(OpenShop);
        }
        private void MarkDirty() { _dirty = true; }
        private void LateUpdate()
        {
            if (_dirty) { _dirty = false; Rebuild(); }
            if (_manager != null)
                foreach (var slot in _slots) slot.Refresh(_manager);
        }
        private void Rebuild()
        {
            foreach (var card in _cards) { if (card != null) { card.gameObject.SetActive(false); Destroy(card.gameObject); } }
            _cards.Clear(); _items.Clear();
            foreach (var slot in _slots) { if (slot != null) { slot.gameObject.SetActive(false); Destroy(slot.gameObject); } }
            _slots.Clear();
            if (_manager == null || _inventory == null) return;
            foreach (var item in _manager.Catalog.Where(i => i != null && _inventory.GetQuantity(i) > 0).Distinct())
            {
                var card = Instantiate(cardPrefab, content);
                card.gameObject.SetActive(true);
                card.Setup(item, SelectItem);
                _cards.Add(card); _items.Add(item);
            }
            if (!_items.Contains(_selected)) _selected = _items.FirstOrDefault();
            if (slotsContainer != null && slotPrefab != null)
                for (int i = 0; i < _manager.SlotCount; i++)
                {
                    var slot = Instantiate(slotPrefab, slotsContainer);
                    slot.gameObject.SetActive(true); slot.Setup(i, SlotClicked); _slots.Add(slot);
                }
            if (emptyText != null) emptyText.gameObject.SetActive(_items.Count == 0);
            RefreshSelection();
        }
        public void SelectItem(SummoningItemData item) { _selected = item; RefreshSelection(); }
        private void RefreshSelection()
        {
            for (int i = 0; i < _cards.Count; i++) _cards[i].Refresh(_items[i] == _selected);
            bool hasItem = _selected != null;
            itemIcon.sprite = hasItem ? _selected.icon : null;
            itemIcon.enabled = hasItem && _selected.icon != null;
            nameText.text = hasItem ? _selected.displayName : "Choose a summoning item";
            descriptionText.text = hasItem ? _selected.description : "Buy an item with Essence, then place it in an empty altar.";
            durationText.text = hasItem ? "Preparation time: " + FormatTime(_selected.summonDurationSeconds) : "";
            possibleMonstersText.text = hasItem ? "Possible monsters\n" + string.Join("\n", _selected.possibleMonsters.Where(e => e != null && e.monster != null && e.weight > 0).Select(e => e.monster.displayName)) : "";
            useButton.interactable = hasItem && _manager != null && _manager.FirstEmptySlot() >= 0;
        }
        public void Begin()
        {
            if (_manager == null || _selected == null) return;
            int slot = _targetSlot >= 0 && _manager.GetSlot(_targetSlot) == null ? _targetSlot : _manager.FirstEmptySlot();
            if (_manager.TryBegin(slot, _selected, out string message))
            {
                _targetSlot = -1;
                collection.ShowSummoning();
            }
            feedbackText.text = message;
            MarkDirty();
        }
        private void SlotClicked(int index)
        {
            if (_manager.GetSlot(index) == null)
            {
                _targetSlot = index;
                feedbackText.text = _selected == null ? "Get an item from the Essence Shop to begin." : "Choose your item, then press Start summoning.";
                return;
            }
            if (_manager.TryCollect(index, out var monster, out string message))
            { feedbackText.text = "Monster added to your collection."; resultWindow.Show(monster); }
            else feedbackText.text = message;
        }
        private void OpenShop() { shop.OpenWindow(); }
        public static string FormatTime(double seconds)
        {
            var time = TimeSpan.FromSeconds(Math.Max(0, Math.Ceiling(seconds)));
            return time.TotalHours >= 1 ? ((int)time.TotalHours).ToString("00") + ":" + time.ToString(@"mm\:ss") : time.ToString(@"mm\:ss");
        }
    }
}