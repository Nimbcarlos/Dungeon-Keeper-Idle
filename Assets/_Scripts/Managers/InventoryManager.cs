using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Lista de Monstros Descobertos/Cadastrados no Jogo")]
        [SerializeField] private List<MonsterData> _allDiscoveredMonsters = new List<MonsterData>();

        private Dictionary<MonsterData, int> _ownedMonsters = new Dictionary<MonsterData, int>();
        private Dictionary<MonsterSlot, MonsterData> _equippedSlots = new Dictionary<MonsterSlot, MonsterData>();

        public IReadOnlyList<MonsterData> AllDiscoveredMonsters => _allDiscoveredMonsters;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
        }

        public int GetAvailableCount(MonsterData data)
        {
            if (data == null) return 0;

            if (!_ownedMonsters.ContainsKey(data)) 
                _ownedMonsters[data] = 1; // Padrão de teste: 1 no estoque inicial

            int owned = _ownedMonsters[data];
            int equipped = 0;

            foreach (var kvp in _equippedSlots)
            {
                if (kvp.Value == data) equipped++;
            }

            return Mathf.Max(0, owned - equipped);
        }

        public void EquipMonsterInSlot(MonsterSlot slot, MonsterData data)
        {
            if (slot == null) return;

            if (_equippedSlots.ContainsKey(slot))
            {
                _equippedSlots.Remove(slot);
            }

            if (data != null && GetAvailableCount(data) > 0)
            {
                _equippedSlots[slot] = data;
                slot.ApplyEquippedMonster(data);
            }
        }

        public void UnequipSlot(MonsterSlot slot)
        {
            if (slot == null) return;

            if (_equippedSlots.ContainsKey(slot))
            {
                _equippedSlots.Remove(slot);
                slot.ClearSlot();
            }
        }
    }
}