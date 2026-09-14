using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonKeeper
{
    [Serializable]
    public class SummoningSaveEntry
    {
        public int slotIndex;
        public string itemID;
        public double startedUtc;
        public double completesUtc;
        public MonsterInstance result;
    }

    public class SummoningManager : MonoBehaviour
    {
        public static SummoningManager Instance { get; private set; }
        [SerializeField] private List<SummoningItemData> _catalog = new();
        [SerializeField, Min(1)] private int _slotCount = 1;
        [SerializeField, Min(1)] private int _monsterCapacity = 10;
        private List<SummoningSaveEntry> _entries = new();
        private double _utcAnchor;
        private double _realtimeAnchor;
        private bool _busy;
        public bool IsBusy => _busy;
        public int SlotCount => Mathf.Max(_slotCount, _entries.Count == 0 ? 1 : _entries.Max(e => e.slotIndex) + 1);
        public int MonsterCapacity => _monsterCapacity;
        public IReadOnlyList<SummoningItemData> Catalog => _catalog;
        public event Action Changed;
        // Monotonic while the application runs; UTC allows offline progress.
        // This is intentionally not an authoritative anti-cheat clock.
        public double Now => _utcAnchor + Math.Max(0, Time.realtimeSinceStartupAsDouble - _realtimeAnchor);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            SynchronizeClock();
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        private void SynchronizeClock()
        {
            _utcAnchor = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            _realtimeAnchor = Time.realtimeSinceStartupAsDouble;
        }
        private void OnApplicationPause(bool paused)
        {
            if (!paused)
            {
                _utcAnchor = Math.Max(Now, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d);
                _realtimeAnchor = Time.realtimeSinceStartupAsDouble;
                Notify();
            }
        }
        public SummoningItemData FindItem(string id) => _catalog.Find(i => i != null && i.itemID == id);
        public SummoningSaveEntry GetSlot(int index) => _entries.Find(e => e.slotIndex == index);
        public double Remaining(int index) => Math.Max(0, (GetSlot(index)?.completesUtc ?? Now) - Now);
        public int FirstEmptySlot()
        {
            for (int i = 0; i < SlotCount; i++) if (GetSlot(i) == null) return i;
            return -1;
        }
        public bool TryBegin(int index, SummoningItemData item, out string message)
        {
            message = "";
            if (_busy) { message = "Please wait."; return false; }
            if (index < 0 || index >= SlotCount || GetSlot(index) != null)
            { message = "This summoning slot is occupied."; return false; }
            if (item == null || !_catalog.Contains(item) || !item.TryValidate(out _) ||
                _catalog.Count(i => i != null && i.itemID == item.itemID) != 1)
            { message = "This summoning item is unavailable."; return false; }
            var items = SummoningItemInventory.Instance;
            var monsters = InventoryManager.Instance;
            if (items == null || monsters == null || items.GetQuantity(item) < 1)
            { message = "Buy a summoning item from the Essence Shop first."; return false; }
            if (item.possibleMonsters.Any(e => monsters.GetDatabase() == null ||
                monsters.GetDatabase().GetMonsterDataByID(e.monster.id) != e.monster))
            { message = "A possible monster is missing from the database."; return false; }

            _busy = true;
            try
            {
                // Roll once and persist the individual before collection: restarting never rerolls it.
                var result = Roll(item);
                var start = Now;
                var entry = new SummoningSaveEntry { slotIndex = index, itemID = item.itemID,
                    startedUtc = start, completesUtc = start + item.summonDurationSeconds, result = result };
                if (!items.TryConsume(item)) { message = "You no longer own this item."; return false; }
                _entries.Add(entry);
            }
            finally { _busy = false; }
            SaveManager.Instance?.SaveGame();
            Notify();
            message = "Summoning started. You can close this window.";
            return true;
        }
        public bool TryCollect(int index, out MonsterInstance result, out string message)
        {
            result = null; message = "";
            var entry = GetSlot(index);
            var inventory = InventoryManager.Instance;
            if (_busy || entry == null || Remaining(index) > 0)
            { message = "This summon is not ready yet."; return false; }
            if (inventory == null || entry.result == null ||
                entry.result.GetData(inventory.GetDatabase()) == null)
            { message = "Monster data is unavailable. Your summon has been kept."; return false; }
            if (inventory.OwnedInstances.Count >= _monsterCapacity)
            { message = "Monster collection is full. Your summon will stay ready."; return false; }
            _busy = true;
            try
            {
                result = entry.result;
                // Remove before inventory notification so saves never capture a claimable duplicate.
                _entries.Remove(entry);
                if (!inventory.OwnedInstances.Any(m => m != null && m.instanceID == entry.result.instanceID))
                    inventory.AddMonsterInstance(result);
            }
            finally { _busy = false; SaveManager.Instance?.SaveGame(); }
            Notify();
            return true;
        }
        public List<SummoningSaveEntry> CaptureState() => _entries.Select(e => JsonUtility.FromJson<SummoningSaveEntry>(JsonUtility.ToJson(e))).ToList();
        public void RestoreState(List<SummoningSaveEntry> entries)
        {
            _entries = new List<SummoningSaveEntry>();
            if (entries != null)
                foreach (var entry in entries)
                {
                    if (entry == null || entry.slotIndex < 0 || entry.slotIndex > 99 ||
                        string.IsNullOrEmpty(entry.itemID) || entry.result == null ||
                        double.IsNaN(entry.completesUtc) || double.IsInfinity(entry.completesUtc) ||
                        GetSlot(entry.slotIndex) != null) continue;
                    _entries.Add(JsonUtility.FromJson<SummoningSaveEntry>(JsonUtility.ToJson(entry)));
                }
            Notify();
        }
        private void Notify()
        {
            if (Changed == null) return;
            foreach (Action listener in Changed.GetInvocationList())
                try { listener(); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
        private static T Weighted<T>(IList<T> values, Func<T, int> weight)
        {
            double total = values.Sum(v => (double)weight(v));
            double roll = UnityEngine.Random.value * total;
            T last = default;
            foreach (var value in values)
            {
                int w = weight(value);
                if (w <= 0) continue;
                last = value;
                roll -= w;
                if (roll < 0) return value;
            }
            return last;
        }
        private static MonsterInstance Roll(SummoningItemData item)
        {
            var data = Weighted(item.possibleMonsters, e => e.weight).monster;
            var quality = Weighted(item.qualityWeights, e => e.weight).quality;
            var modifiers = MonsterGenerator.GenerateAffixesForQuality(quality);
            if (item.preferredAffixes != null && item.preferredAffixes.Count > 0)
                for (int i = 0; i < modifiers.modifiers.Count; i++)
                {
                    var modifier = modifiers.modifiers[i];
                    modifier.type = Weighted(item.preferredAffixes, e => e.weight).attribute;
                    modifiers.modifiers[i] = modifier;
                }
            var instance = new MonsterInstance(data.id, quality) { affixes = modifiers.modifiers, behaviors = modifiers.behaviors };
            instance.progression.ResolveTalents(data, 10 * ((int)quality + 1));
            return instance;
        }
    }
}

