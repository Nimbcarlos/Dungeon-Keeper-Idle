using UnityEngine;

namespace DungeonKeeper
{
    [System.Serializable]
    public class Stats
    {
        public int maxHP = 100;
        public int attackPower = 10;
        public float attackSpeed = 1f;
        public float detectionRange = 4f;
        public float attackRange = 1.5f;
        public float moveSpeed = 2f;

        public Stats Clone()
        {
            return new Stats
            {
                maxHP        = this.maxHP,
                attackPower  = this.attackPower,
                attackSpeed  = this.attackSpeed,
                attackRange  = this.attackRange,
                moveSpeed    = this.moveSpeed
            };
        }
    }
}