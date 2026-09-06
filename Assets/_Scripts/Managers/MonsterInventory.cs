using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterInventory : MonoBehaviour
    {
        public static MonsterInventory Instance { get; private set; }

        [SerializeField] private MonsterDatabase _database;
        private List<MonsterInstance> _ownedMonsters = new List<MonsterInstance>();

        public IReadOnlyList<MonsterInstance> OwnedMonsters => _ownedMonsters;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AddMonster(MonsterInstance instance)
        {
            if (instance == null) return;
            _ownedMonsters.Add(instance);
            Debug.Log($"📦 [Inventory] Monstro {instance.monsterDataID} ({instance.instanceID}) adicionado ao inventário!");
        }

        public MonsterInstance GetInstanceByID(string instanceID)
        {
            return _ownedMonsters.Find(m => m.instanceID == instanceID);
        }

        public void LoadSavedInventory(List<MonsterInstance> loadedMonsters)
        {
            _ownedMonsters = loadedMonsters ?? new List<MonsterInstance>();
        }
    }
}