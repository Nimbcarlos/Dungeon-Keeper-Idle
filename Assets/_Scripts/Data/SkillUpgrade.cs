using UnityEngine;
using DungeonKeeper;

[CreateAssetMenu(fileName = "SkillUpgrade", menuName = "Dungeon/Skill Upgrade")]
public class SkillUpgrade : ScriptableObject
{
    [Header("Identificação")]
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;

    [Header("Modificadores")]
    public bool  enablePiercing   = false;
    public bool  enableBounce     = false;
    public bool  enableVolley     = false;
    public bool  enableHoming     = false;
    public int   volleyCountBonus = 0;
    public float spreadAngleBonus = 0f;
    public int   damageBonus      = 0;
    public float speedBonus       = 0f;
    public float cooldownReduction= 0f;
    public int   maxBounceBonus   = 0;

    [Header("Próximas escolhas")]
    public SkillUpgrade[] nextOptions; // opções que aparecem no próximo nível
}