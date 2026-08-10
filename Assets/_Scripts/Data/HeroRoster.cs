using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonKeeper;

[CreateAssetMenu(fileName = "HeroRoster", menuName = "Dungeon/Hero Roster")]
public class HeroRoster : ScriptableObject
{
    [System.Serializable]
    public class HeroEntry
    {
        public HeroData data;

        [Header("Condições de entrada")]
        public int   minGold       = 0;
        public int   maxGold       = 99999;
        public int   minDifficulty = 1;

        [Header("Chance")]
        [Range(1, 100)]
        public int weight = 50; // peso de aparecer
    }

    public List<HeroEntry> heroes = new List<HeroEntry>();

    public HeroData GetRandom(int currentGold, int difficulty)
    {
        // filtra heróis disponíveis nas condições atuais
        List<HeroEntry> available = heroes
            .Where(h => currentGold  >= h.minGold
                     && currentGold  <= h.maxGold
                     && difficulty   >= h.minDifficulty)
            .ToList();

        if (available.Count == 0) return null;

        // rolagem por peso
        int totalWeight = available.Sum(h => h.weight);
        int roll        = Random.Range(0, totalWeight);
        int accumulated = 0;

        foreach (HeroEntry entry in available)
        {
            accumulated += entry.weight;
            if (roll < accumulated)
                return entry.data;
        }

        return available[0].data;
    }
}