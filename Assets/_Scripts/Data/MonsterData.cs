using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Dungeon/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Identificação")]
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

    [Header("Stats")]
    public Stats stats;

    [Header("Loot")]
    public int goldReward    = 5;
    public int essenceReward = 1;
    public MonsterBehavior defaultBehavior = MonsterBehavior.Defensive;
}

public enum MonsterBehavior
{
    Defensive,    // fica no slot, ataca quem entra no range (Slime atual)
    Aggressive,   // sai para interceptar, não volta até matar
    Ranged,       // mantém distância mínima, recua de melee
    Support,      // prioriza curar aliados
    Custom        // jogador configura
}