using UnityEngine;

namespace DungeonKeeper
{
    public abstract class HeroRoleStrategy
    {
        public HeroBrain Brain { get; private set; }

        public virtual void Initialize(HeroBrain brain)
        {
            Brain = brain;
        }

        public abstract Character FindTarget();
        public abstract Vector2 GetTargetPosition(Character target);
        public abstract void ExecuteAction(Character target);

        protected Character FindMonsterTarget()
        {
            Monster[] aliveMonsters = Brain.GetAliveEntities<Monster>();

            if (aliveMonsters == null || aliveMonsters.Length == 0) 
                return null;

            float sightRange = (this is RangedRoleStrategy) 
                               ? Brain.BaseCharacter.Stats.attackRange + 1.5f 
                               : 3.5f;

            // Filtra o radar verificando Lane (Y) e Alcance (X)
            Character[] monstersInSight = System.Array.FindAll(aliveMonsters, m => 
            {
                Vector2 mPos = Brain.GetEntityFeetPos(m);
                bool isSameLane = Mathf.Abs(Brain.MyFeetPos.y - mPos.y) < 0.5f;
                float dist = Vector2.Distance(Brain.MyFeetPos, mPos);
                
                return isSameLane && dist <= sightRange;
            });

            if (monstersInSight.Length == 0) 
                return null;

            if (Brain.TargetStrategy != null)
                return Brain.TargetStrategy.SelectTarget(Brain.BaseCharacter, monstersInSight);

            return FindClosestFallback(monstersInSight);
        }

        private Character FindClosestFallback(Character[] targets)
        {
            Vector2 myPos = Brain.MyFeetPos;
            Character closest = null;
            float minDist = float.MaxValue;

            foreach (Character t in targets)
            {
                float dist = Vector2.Distance(myPos, Brain.GetEntityFeetPos(t));
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = t;
                }
            }
            return closest;
        }
    }

    // =========================================================
    // COMPORTAMENTO MELEE
    // =========================================================
    public class MeleeRoleStrategy : HeroRoleStrategy
    {
        public override Character FindTarget() => FindMonsterTarget();

        public override Vector2 GetTargetPosition(Character target)
        {
            Vector2 targetPos = Brain.GetEntityFeetPos(target);
            int meleeIndex = GetMeleeQueueIndex(target);

            switch (meleeIndex)
            {
                case 0: return new Vector2(targetPos.x - 0.75f, targetPos.y);
                case 1: return new Vector2(targetPos.x - 0.85f, targetPos.y + 0.60f);
                case 2: return new Vector2(targetPos.x - 0.85f, targetPos.y - 0.60f);
                default: return new Vector2(targetPos.x - (0.75f + (meleeIndex * 0.4f)), targetPos.y);
            }
        }

        public override void ExecuteAction(Character target)
        {
            Brain.BaseCharacter.Animator?.SetTrigger("Slash");
            Brain.BaseCharacter.Attack(target);
        }

        private int GetMeleeQueueIndex(Character target)
        {
            Hero[] heroes = Object.FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            int index = 0;

            foreach (Hero h in heroes)
            {
                if (h == Brain.BaseCharacter || h == null || !h.IsAlive) continue;

                HeroBrain otherBrain = h.GetComponent<HeroBrain>();
                if (otherBrain != null && otherBrain.ActiveStrategy is MeleeRoleStrategy)
                {
                    Vector2 otherPos = otherBrain.GetEntityFeetPos(h);
                    Vector2 targetPos = Brain.GetEntityFeetPos(target);

                    float myDist = Vector2.Distance(Brain.MyFeetPos, targetPos);
                    float otherDist = Vector2.Distance(otherPos, targetPos);

                    if (otherDist < myDist - 0.05f) index++;
                    else if (Mathf.Abs(myDist - otherDist) <= 0.05f)
                    {
                        if (h.gameObject.GetEntityId() < Brain.gameObject.GetEntityId())
                            index++;
                    }
                }
            }
            return index;
        }
    }

    // =========================================================
    // COMPORTAMENTO RANGED
    // =========================================================
    public class RangedRoleStrategy : HeroRoleStrategy
    {
        public override Character FindTarget() => FindMonsterTarget();

        public override Vector2 GetTargetPosition(Character target)
        {
            Vector2 targetPos = Brain.GetEntityFeetPos(target);
            
            float safeDistance = Brain.BaseCharacter.Stats.attackRange * 0.8f;
            if (safeDistance <= 0.5f) safeDistance = 3f;

            // Evita erro caso o herói e monstro estejam perfeitamente no mesmo X
            float diffX = Brain.MyFeetPos.x - targetPos.x;
            float directionSign = Mathf.Abs(diffX) < 0.01f ? -1f : Mathf.Sign(diffX);

            return new Vector2(targetPos.x + (directionSign * safeDistance), targetPos.y);
        }

        public override void ExecuteAction(Character target)
        {
            Brain.BaseCharacter.Animator?.SetTrigger("SimpleBowShot");
            Brain.BaseCharacter.Attack(target);
        }
    }
}