using UnityEngine;

namespace DungeonKeeper
{
    public enum SkillType
    {
        // Status Base
        FlatDamage,
        PercentDamage,
        FlatHealth,
        PercentHealth,
        AttackSpeed,

        // Mecânicas de Projétil / Combate (Marcos Level 5, 10...)
        SpecialEffect,    // Habilidade Passiva / Efeito unico
        Piercing,         // Atravessa N alvos
        Ricochet,         // Rebate N vezes
        BurnOnHit,        // Aplica dano contínuo de Fogo
        StunChance,       // Chance de atordoar (Eletricidade)
        SlowOnHit         // Aplica lentidão (Água/Gelo)

    }

    [CreateAssetMenu(fileName = "SO_SkillNode", menuName = "Dungeon/Skill Node")]
    public class SkillNodeSO : ScriptableObject
    {
        [Header("Informações do Nó")]
        public string skillID;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Requisitos")]
        public int requiredMonsterLevel = 1;
        public int skillPointCost = 1;
        public SkillNodeSO requiredParentSkill; // Habilidade pré-requisito na árvore

        [Header("Efeito nos Status")]
        public SkillType skillType;
        public float modifierValue; // Ex: 10 para +10 HP ou 0.15 para +15%
        [Header("Regras de Escolha")]
        public SkillNodeSO mutuallyExclusiveSkill; // O nó oposto na mesma linha (ex: Node B)
    }
}