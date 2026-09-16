using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    [CreateAssetMenu(fileName = "AttackSpecialization", menuName = "Dungeon/Attack Specialization")]
    public class AttackSpecialization : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        [Header("Visual na árvore")]
        [Tooltip("Ícone do ataque no primeiro nó da árvore. Arraste um Sprite aqui. Se vazio, usa o ícone do monstro.")]
        public Sprite icon;

        [Header("Configuração do ataque")]
        public AttackType attackType;
        [Min(0)] public int rollWeight = 1;
        [Min(0.1f)] public float attackRange = 1.5f;
        public MeleeSkillData meleeData;
        public ProjectileData projectileData;
        public List<SkillNodeSO> upgradePool = new List<SkillNodeSO>();

        public bool IsConfigured => !string.IsNullOrEmpty(id) &&
            (attackType == AttackType.Melee ? meleeData != null :
             projectileData != null && projectileData.prefab != null);
    }
}
