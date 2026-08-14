using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterBrain : CharacterBrain
    {
        public enum BrainState { Idle, Alert, Combat, Returning }

        [Header("Configurações do Cérebro")]
        [SerializeField] private BrainState _state = BrainState.Idle;

        private float   _attackTimer;
        private bool    _isAttacking;
        private Monster _monster;

        private const float Y_THRESHOLD = 0.12f;

        private Vector2 MyFeetPos => character.FeetPoint != null ? (Vector2)character.FeetPoint.position : (Vector2)transform.position;

        protected override void Awake()
        {
            base.Awake();
            _monster = GetComponent<Monster>();
        }

        protected override void Think()
        {
            if (_isAttacking || _monster == null || !_monster.IsAlive) return;

            Hero closest = FindClosestHero();

            switch (_state)
            {
                case BrainState.Idle:      HandleIdle(closest);      break;
                case BrainState.Alert:     HandleAlert(closest);     break;
                case BrainState.Combat:    HandleCombat(closest);    break;
                case BrainState.Returning: HandleReturning(closest); break;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // ⚙️ MÁQUINA DE ESTADOS (APENAS DECISÃO)
        // ─────────────────────────────────────────────────────────────────

        private void HandleIdle(Hero closest)
        {
            ExecuteMovement(Vector2.zero, 0f);

            if (closest != null && IsInDetectionRange(closest))
            {
                SetState(BrainState.Alert);
            }
        }

        private void HandleAlert(Hero closest)
        {
            if (closest == null || !closest.IsAlive || !IsInDetectionRange(closest))
            {
                SetState(BrainState.Returning);
                return;
            }

            Vector2 targetFeetPos = GetEntityFeetPos(closest);

            if (IsTargetInAttackRange(targetFeetPos))
            {
                SetState(BrainState.Combat);
                return;
            }

            Vector2 moveDir = CalculateTwoPhaseMovement(MyFeetPos, targetFeetPos);
            float lookDir = moveDir.x != 0 ? moveDir.x : (targetFeetPos.x - MyFeetPos.x);

            ExecuteMovement(moveDir, lookDir);
        }

        private void HandleCombat(Hero closest)
        {
            if (closest == null || !closest.IsAlive)
            {
                StopAttack();
                SetState(BrainState.Returning);
                return;
            }

            Vector2 targetFeetPos = GetEntityFeetPos(closest);

            if (!IsTargetInAttackRange(targetFeetPos))
            {
                StopAttack();
                SetState(BrainState.Alert);
                return;
            }

            // No combate: fica cravado no chão e apenas olha para o herói
            ExecuteMovement(Vector2.zero, targetFeetPos.x - MyFeetPos.x);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f && !_isAttacking)
            {
                _attackTimer = 1f / character.Stats.attackSpeed;
                StartCoroutine(AttackRoutine(closest));
            }
        }

        private void HandleReturning(Hero closest)
        {
            if (closest != null && IsInDetectionRange(closest))
            {
                SetState(BrainState.Alert);
                return;
            }

            Vector2 slotPos = _monster.GuardPosition;

            if (Vector2.Distance(MyFeetPos, slotPos) <= 0.05f)
            {
                transform.position = slotPos;
                ExecuteMovement(Vector2.zero, 1f); // Olha para a direita ao chegar
                SetState(BrainState.Idle);
                return;
            }

            Vector2 moveDir = CalculateTwoPhaseMovement(MyFeetPos, slotPos);
            float lookDir = moveDir.x != 0 ? moveDir.x : (slotPos.x - MyFeetPos.x);

            ExecuteMovement(moveDir, lookDir);
        }

        // ─────────────────────────────────────────────────────────────────
        // 🚀 EXECUÇÃO CENTRALIZADA DE MOVIMENTO E VISUAL (CÉREBRO)
        // ─────────────────────────────────────────────────────────────────

        public void SetState(BrainState newState)
        {
            _state = newState;
        }

        /// <summary>
        /// PONTO ÚNICO DE SAÍDA: Gerencia Move, Flip/Scale e Animação de Movimento
        /// </summary>
        private void ExecuteMovement(Vector2 moveDirection, float lookDirectionX)
        {
            ApplyVisualFlip(lookDirectionX);

            if (moveDirection == Vector2.zero)
            {
                character.Animator?.SetBool("isMoving", false);
                character.Move(Vector2.zero);
                return;
            }

            character.Animator?.SetBool("isMoving", true);
            character.Move(moveDirection);
        }

        private Vector2 CalculateTwoPhaseMovement(Vector2 currentPos, Vector2 targetPos)
        {
            float deltaY = targetPos.y - currentPos.y;
            float deltaX = targetPos.x - currentPos.x;

            if (Mathf.Abs(deltaY) > Y_THRESHOLD)
                return new Vector2(0f, Mathf.Sign(deltaY));

            if (Mathf.Abs(deltaX) > 0.05f)
                return new Vector2(Mathf.Sign(deltaX), 0f);

            return Vector2.zero;
        }

        private void ApplyVisualFlip(float directionX)
        {
            if (Mathf.Abs(directionX) < 0.01f) return;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(directionX) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        private void StopAttack()
        {
            StopAllCoroutines();
            _isAttacking = false;
            _attackTimer = 0f;
        }

        // ─────────────────────────────────────────────────────────────────
        // 📏 CHECAGENS DE ALCANCE E PIVÔS
        // ─────────────────────────────────────────────────────────────────

        private bool IsInDetectionRange(Hero hero)
        {
            if (hero == null || !hero.IsAlive) return false;
            return Vector2.Distance(MyFeetPos, GetEntityFeetPos(hero)) <= character.Stats.detectionRange;
        }

        private bool IsTargetInAttackRange(Vector2 targetFeetPos)
        {
            float diffX = Mathf.Abs(MyFeetPos.x - targetFeetPos.x);
            float diffY = Mathf.Abs(MyFeetPos.y - targetFeetPos.y);
            return diffX <= character.Stats.attackRange && diffY <= Y_THRESHOLD;
        }

        private Hero FindClosestHero()
        {
            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            Hero closest  = null;
            float minDist = float.MaxValue;

            foreach (Hero h in heroes)
            {
                if (h == null || !h.IsAlive) continue;
                float dist = Vector2.Distance(MyFeetPos, GetEntityFeetPos(h));
                if (dist < minDist) { minDist = dist; closest = h; }
            }
            return closest;
        }

        private IEnumerator AttackRoutine(Hero target)
        {
            _isAttacking = true;
            character.Animator?.SetTrigger("attack");

            float totalDuration = 1f / character.Stats.attackSpeed;
            float impactTime    = totalDuration * 0.4f;
            float recoveryTime  = totalDuration * 0.6f;

            yield return new WaitForSeconds(impactTime);

            if (target != null && target.IsAlive)
                character.Attack(target);

            yield return new WaitForSeconds(recoveryTime);
            _isAttacking = false;
        }

        private Vector2 GetEntityFeetPos(Character entity)
        {
            return entity.FeetPoint != null ? (Vector2)entity.FeetPoint.position : (Vector2)entity.transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            if (character == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(MyFeetPos, character.Stats.detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(MyFeetPos, character.Stats.attackRange);
        }
    }
}