using UnityEngine;

namespace DungeonKeeper
{
    [CreateAssetMenu(fileName = "NewMeleeData", menuName = "Dungeon/Melee Data")]
    public class MeleeSkillData : ScriptableObject
    {
        [Header("Visual & SFX")]
        public Sprite attackVFXSprite;
        public AudioClip swingSFX;
        public AudioClip hitSFX;

        [Header("Combat Stats")]
        public int baseDamage = 15;
        public float attackRange = 1.5f;       // Alcance do golpe
        public float attackArcAngle = 90f;      // Ângulo do corte em arco
        public int maxCleaveTargets = 1;        // Quantos inimigos acerta de uma vez (1 = Single Target)
        public float knockbackForce = 2f;
        public float cooldown = 2f;

        [Header("Damage Type")]
        public DamageType damageType = DamageType.Physical;
    }
}