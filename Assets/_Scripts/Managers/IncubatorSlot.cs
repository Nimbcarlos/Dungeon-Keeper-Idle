using System;
using UnityEngine;

namespace DungeonKeeper
{
    [System.Serializable]
    public class IncubatorSlot
    {
        public int slotIndex;
        public EggData eggData;
        public float remainingTime;
        public bool isOccupied;
        public bool isReadyToHatch;

        public IncubatorSlot(int index)
        {
            slotIndex = index;
            eggData = null;
            remainingTime = 0f;
            isOccupied = false;
            isReadyToHatch = false;
        }

        public void PlaceEgg(EggData egg)
        {
            eggData = egg;
            remainingTime = egg != null ? egg.hatchTimeSeconds : 0f;
            isOccupied = egg != null;
            isReadyToHatch = false;
        }

        public void UpdateTimer(float deltaTime)
        {
            if (!isOccupied || isReadyToHatch) return;

            remainingTime -= deltaTime;
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isReadyToHatch = true;
            }
        }

        public void ClearSlot()
        {
            eggData = null;
            remainingTime = 0f;
            isOccupied = false;
            isReadyToHatch = false;
        }
    }
}