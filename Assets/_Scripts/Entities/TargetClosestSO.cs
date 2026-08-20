using UnityEngine;

namespace DungeonKeeper
{
    [CreateAssetMenu(fileName = "SO_TargetClosest", menuName = "Dungeon/AI/Strategies/Target Closest")]
    public class TargetClosestSO : TargetSelectionSO
    {
        public override Character SelectTarget(Character self, Character[] potentialTargets)
        {
            if (self == null || potentialTargets == null || potentialTargets.Length == 0) return null;

            Vector2 selfPos = self.FeetPoint != null ? (Vector2)self.FeetPoint.position : (Vector2)self.transform.position;
            Character closest = null;
            float minDist = float.MaxValue;

            foreach (Character target in potentialTargets)
            {
                if (target == null || !target.IsAlive) continue;

                Vector2 targetPos = target.FeetPoint != null ? (Vector2)target.FeetPoint.position : (Vector2)target.transform.position;
                float dist = Vector2.Distance(selfPos, targetPos);

                if (dist < minDist)
                {
                    minDist = dist;
                    closest = target;
                }
            }

            return closest;
        }
    }
}