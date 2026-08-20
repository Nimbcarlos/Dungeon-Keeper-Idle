using UnityEngine;

namespace DungeonKeeper
{
    [CreateAssetMenu(fileName = "SO_TargetWeakest", menuName = "Dungeon/AI/Strategies/Target Weakest")]
    public class TargetWeakestSO : TargetSelectionSO
    {
        public override Character SelectTarget(Character self, Character[] potentialTargets)
        {
            if (self == null || potentialTargets == null || potentialTargets.Length == 0) return null;

            Character weakest = null;
            float lowestHpPercent = 1.05f; // Inicia acima de 100%

            foreach (Character target in potentialTargets)
            {
                if (target == null || !target.IsAlive || target.Stats == null) continue;

                float hpPercent = (float)target.Health.Percent / target.Stats.maxHP;

                if (hpPercent < lowestHpPercent)
                {
                    lowestHpPercent = hpPercent;
                    weakest = target;
                }
            }

            return weakest;
        }
    }
}