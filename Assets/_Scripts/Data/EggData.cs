using DungeonKeeper;

[System.Serializable]
public struct StatWeight
{
    public SkillType statType;
    public int weight; // Quanto maior o peso, maior a chance de ser sorteado

    // No sorteio dos atributos do monstro:
    public static SkillType GetRandomStatByQuality(
        MonsterQuality quality,
        StatWeight[] availableStats)
    {
        if (availableStats == null || availableStats.Length == 0)
            throw new System.ArgumentException("Nenhum atributo disponível.", nameof(availableStats));

        int totalWeight = 0;
        foreach (StatWeight stat in availableStats)
            totalWeight += System.Math.Max(0, stat.weight);

        if (totalWeight == 0)
            return availableStats[0].statType;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (StatWeight stat in availableStats)
        {
            roll -= System.Math.Max(0, stat.weight);
            if (roll < 0)
                return stat.statType;
        }

        return availableStats[availableStats.Length - 1].statType;
    }
}