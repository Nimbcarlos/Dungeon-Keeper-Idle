using UnityEngine;

namespace DungeonKeeper
{
    public enum SkillType
    {
// 🛡️ Status Base
        FlatDamage,
        PercentDamage,
        FlatHealth,
        PercentHealth,
        AttackSpeed,
        MovementSpeed,
        Defense,

        // ⚔️ Atributos Ofensivos Avançados
        CritChance,          // Chance de Acerto Crítico
        CritDamage,          // Dano Crítico Ampliado
        ArmorPenetration,    // Penetração de Armadura
        ProjectileSpeed,     // Velocidade do Projétil
        AoERadius,           // Raio do Dano em Área

        // 🛡️ Resistências & Defesa
        ResistFire,          // Resistência a Fogo
        ResistIce,           // Resistência a Gelo/Lentidão
        ResistPoison,        // Resistência a Veneno
        ResistLight,
        ResistChaos,
        ResistLightning,     // Resistência a Eletricidade/Stun
        ResistElemental,           // Resistência Geral/Elemental

        // 🩸 Sustentabilidade & Efeitos
        LifeLeech,           // Roubo de Vida
        HealthRegen,         // Regeneração de HP por Segundo
        ThornsDamage,        // Reflexão de Dano
        CooldownReduction,   // Redução de Tempo de Recarga

        // 🌟 Mecânicas de Projétil / Combate Único
        SpecialEffect,       // Habilidade Passiva / Efeito único
        Piercing,            // Atravessa N alvos
        Ricochet,            // Rebate N vezes
        BurnOnHit,           // Aplica dano contínuo de Fogo
        StunChance,          // Chance de atordoar (Eletricidade)
        SlowOnHit            // Aplica lentidão (Água/Gelo)

    }

    [CreateAssetMenu(fileName = "SO_SkillNode", menuName = "Dungeon/Skill Node")]
    public class SkillNodeSO : ScriptableObject
    {
        [Header("Informações do Nó")]
        public string skillID;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Categoria da Recompensa")]
        public RewardType rewardType = RewardType.ModifierChoice; // Define o comportamento do nó na UI

        [Header("Requisitos")]
        public int requiredMonsterLevel = 1;
        public int skillPointCost = 1;
        public SkillNodeSO requiredParentSkill; // Habilidade pré-requisito na árvore

        [Header("Efeito nos Status")]
        public SkillType skillType;
        public float modifierValue; // Ex: 10 para +10 HP ou 0.15 para +15%[cite: 14]

        [Header("Regras de Escolha")]
        public SkillNodeSO mutuallyExclusiveSkill; // O nó oposto na mesma linha (ex: Node B)[cite: 14]
    }
}