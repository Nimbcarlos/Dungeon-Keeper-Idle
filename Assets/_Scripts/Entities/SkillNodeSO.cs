using UnityEngine;

namespace DungeonKeeper
{
    public enum SkillType
    {
        FlatDamage,       // +5 de Ataque
        PercentDamage,    // +10% de Dano
        FlatHealth,       // +20 de Vida Maxima
        PercentHealth,    // +15% de Vida
        AttackSpeed,      // +10% Velocidade de Ataque
        SpecialEffect     // Habilidade Passiva / Efeito unico
    }

    [CreateAssetMenu(fileName = "SO_SkillNode", menuName = "DungeonKeeper/Skills/Skill Node")]
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
    }
}