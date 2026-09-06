using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    [Serializable]
    public class MonsterInstance
    {
        public string instanceID;      // GUID único no save
        public string monsterDataID;   // Ex: "green_slime"
        public MonsterQuality quality;
        
        public MonsterProgression progression;
        public List<StatModifier> affixes = new List<StatModifier>();
        public List<BehaviorType> behaviors = new List<BehaviorType>();

        // Cache Runtime do Asset (Não Serializado no Save)
        [NonSerialized] private MonsterData _cachedData;

        public MonsterInstance(string dataID, MonsterQuality monsterQuality)
        {
            instanceID = Guid.NewGuid().ToString();
            monsterDataID = dataID;
            quality = monsterQuality;
            progression = new MonsterProgression();
        }

        /// <summary>
        /// Resolve e retorna o MonsterData através de um Database central.
        /// </summary>
        public MonsterData GetData(MonsterDatabase database)
        {
            if (_cachedData == null && database != null)
            {
                _cachedData = database.GetMonsterDataByID(monsterDataID);
            }
            return _cachedData;
        }
    }
}