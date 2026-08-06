using UnityEngine;

[CreateAssetMenu(fileName = "PartyConfig", menuName = "Dungeon/Party Config")]
public class PartyConfig : ScriptableObject
{
    [System.Serializable]
    public class PartyTier
    {
        public string label;        // "Solo", "Dupla", "Trio", "Party"
        public int    minGold;
        public int    maxGold;
        public int    minSize;
        public int    maxSize;      // tamanho aleatório entre min e max
    }

    public PartyTier[] tiers;

    public int GetPartySize(int currentGold)
    {
        foreach (PartyTier tier in tiers)
        {
            if (currentGold >= tier.minGold && currentGold <= tier.maxGold)
                return Random.Range(tier.minSize, tier.maxSize + 1);
        }
        return 1;
    }
}