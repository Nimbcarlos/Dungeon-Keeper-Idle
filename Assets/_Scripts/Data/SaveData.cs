using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    [Serializable]
    public class SaveData
    {
        // 💰 Economia Base
        public int gold;
        public int essence;

        // 🧟 Monstros Ativos nas Lanes
        public List<MonsterSaveState> activeMonsters = new List<MonsterSaveState>();

        // 🎒 Inventário de Ovos e Monstros Não Equipados
        public List<EggSaveState> eggInventory = new List<EggSaveState>();
        public List<string> unlockedMonsterIDs = new List<string>();

        // 🎨 Cosméticos / Skins
        public List<string> unlockedSkinIDs = new List<string>();
    }

    [Serializable]
    public class MonsterSaveState
    {
        public string monsterID;
        public int currentLevel;
        public int currentXP;
        public MonsterQuality quality;

        public Vector3 position;       // Posição 3D do monstro na cena
        public int laneIndex = -1;     // Índice da lane onde ele estava equipado

        // 🌳 IDs dos Nós da SkillTree que o monstro desbloqueou
        public List<string> unlockedSkillIDs = new List<string>();
    }

    [Serializable]
    public class EggSaveState
    {
        public string eggID;
        public float hatchTimer; // Tempo restante de chocagem (se aplicável)
    }
}