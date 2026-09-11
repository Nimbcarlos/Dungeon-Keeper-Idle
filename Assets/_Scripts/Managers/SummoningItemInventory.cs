using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    [Serializable]
    public class SummoningItemSaveEntry
    {
        public string itemID;
        public int quantity;
    }

    public class SummoningItemInventory : MonoBehaviour
    {
        public static SummoningItemInventory Instance { get; private set; }
        public event Action OnInventoryChanged;

        private readonly Dictionary<string, int> _quantities = new(StringComparer.Ordinal);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public int GetQuantity(SummoningItemData item) => item == null ? 0 : GetQuantity(item.itemID);

        public int GetQuantity(string itemID)
        {
            return !string.IsNullOrWhiteSpace(itemID) && _quantities.TryGetValue(itemID, out int count)
                ? count : 0;
        }

        public bool CanAdd(SummoningItemData item, int quantity = 1)
        {
            return item != null && !string.IsNullOrWhiteSpace(item.itemID) && quantity > 0 &&
                (long)GetQuantity(item) + quantity <= int.MaxValue;
        }

        public bool TryAdd(SummoningItemData item, int quantity = 1)
        {
            if (!CanAdd(item, quantity)) return false;
            _quantities[item.itemID] = GetQuantity(item) + quantity;
            NotifyChanged();
            return true;
        }

        public bool TryConsume(SummoningItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0 || GetQuantity(item) < quantity) return false;
            int remaining = GetQuantity(item) - quantity;
            if (remaining == 0) _quantities.Remove(item.itemID);
            else _quantities[item.itemID] = remaining;
            NotifyChanged();
            return true;
        }

        // Snapshot independente: a UI e o save nao recebem o Dictionary mutavel.
        public List<SummoningItemSaveEntry> CaptureState()
        {
            var result = new List<SummoningItemSaveEntry>();
            foreach (var pair in _quantities)
                result.Add(new SummoningItemSaveEntry { itemID = pair.Key, quantity = pair.Value });
            result.Sort((a, b) => string.CompareOrdinal(a.itemID, b.itemID));
            return result;
        }

        public void RestoreState(List<SummoningItemSaveEntry> entries)
        {
            _quantities.Clear();
            if (entries != null)
                foreach (var entry in entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.itemID) || entry.quantity <= 0) continue;
                    // Duplicatas malformadas nao multiplicam o saldo.
                    if (!_quantities.ContainsKey(entry.itemID)) _quantities.Add(entry.itemID, entry.quantity);
                }
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            // Um erro de um listener visual nao deve interromper uma compra concluida.
            if (OnInventoryChanged == null) return;
            foreach (Action listener in OnInventoryChanged.GetInvocationList())
                try { listener(); }
                catch (Exception exception) { Debug.LogException(exception, this); }
        }
    }
}