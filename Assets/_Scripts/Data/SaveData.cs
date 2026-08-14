using System;
using System.Collections.Generic;

namespace DungeonKeeper
{
    [Serializable]
    public class MonsterSaveItem
    {
        public string monsterID; // ID do ScriptableObject
        public int amountOwned;  // Quantidade total comprada
    }

    [Serializable]
    public class SaveData
    {
        public int gold = 100;
        public int essence = 0;
        public List<MonsterSaveItem> monsterInventory = new List<MonsterSaveItem>();
        
        // Mapeia qual monstro está em qual lane (SlotIndex -> MonsterID)
        public Dictionary<int, string> equippedLanes = new Dictionary<int, string>();
    }
}