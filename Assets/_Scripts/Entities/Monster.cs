using UnityEngine;

namespace DungeonKeeper
{
    public class Monster : Character
    {
        public MonsterData Data { get; private set; }
        public Vector3     GuardPosition { get; private set; }
        private static readonly int IsMoving    = Animator.StringToHash("isMoving");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private static readonly int IsHurt      = Animator.StringToHash("isHurt");
        private static readonly int IsDead      = Animator.StringToHash("isDead");

        public void Initialize(MonsterData monsterData)
        {
            Data          = monsterData;
            GuardPosition = transform.position; // salva após Instantiate na posição certa
            base.Initialize(monsterData.stats);
        }


        protected override void OnAttack()
        {
            if (Animator == null) return;
            Animator.SetBool(IsMoving,    false);
            // Animator.SetBool(IsAttacking, true);
        }

        protected override void OnHit()
        {
            // if (Animator == null) return;
            // Animator.SetBool(IsHurt, true);
        }

        protected override void OnDieEffect()
        {
            if (Animator == null) return;
            Animator.SetBool(IsDead, true);
        }
    }
}