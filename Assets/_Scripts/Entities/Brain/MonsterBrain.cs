using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class MonsterBrain : CharacterBrain
    {
        private enum State { Idle, Alert, Combat, Returning }

        private State   _state       = State.Idle;
        private float   _attackTimer;
        private bool    _isAttacking;
        private Monster _monster;     // referência direta

        protected override void Awake()
        {
            base.Awake();
            _monster = GetComponent<Monster>();
        }

        protected override void Think()
        {
            if (_isAttacking) return;

            // GuardPosition já foi definido pelo Initialize — sempre correto
            Hero closest = FindClosestHero();

            switch (_state)
            {
                case State.Idle:      HandleIdle(closest);      break;
                case State.Alert:     HandleAlert(closest);     break;
                case State.Combat:    HandleCombat(closest);    break;
                case State.Returning: HandleReturning(closest); break;
            }
        }

        void HandleIdle(Hero closest)
        {
            character.Animator?.SetBool("isMoving", false);
            if (closest == null) return;

            float dist = Vector2.Distance(character.FeetPoint.position, closest.transform.position);
            if (dist <= character.Stats.detectionRange)
                _state = State.Alert;
        }

        void HandleAlert(Hero closest)
        {
            if (closest == null)
            {
                _state = State.Returning;
                return;
            }

            float dist = Vector2.Distance(character.FeetPoint.position, closest.transform.position);

            if (dist <= character.Stats.attackRange)
            {
                _state = State.Combat;
                return;
            }

            if (dist > character.Stats.detectionRange)
            {
                _state = State.Returning;
                return;
            }

            FaceTarget(closest.transform.position);
            character.Animator?.SetBool("isMoving", true);

            Vector2 dir = ((Vector2)closest.transform.position
                - (Vector2)transform.position).normalized;
            character.Move(dir);
        }

        void HandleCombat(Hero closest)
        {
            character.Animator?.SetBool("isMoving", false);
            if (closest == null || !closest.IsAlive)
            {
                StopAllCoroutines();
                _isAttacking = false;
                _attackTimer = 0f;
                _state       = State.Returning;
                return;
            }

            float dist = Vector2.Distance(character.FeetPoint.position, closest.transform.position);

            if (dist > character.Stats.attackRange)
            {
                StopAllCoroutines();
                _isAttacking = false;
                _state       = State.Alert;
                return;
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f && !_isAttacking)
            {
                _attackTimer = 1f / character.Stats.attackSpeed;
                StartCoroutine(AttackRoutine(closest));
            }
        }

        void HandleReturning(Hero closest)
        {
            Vector3 slot = _monster.GuardPosition; // sempre o valor correto

            // continua vigiando enquanto volta
            if (closest != null)
            {
                float dist = Vector2.Distance(character.FeetPoint.position, closest.transform.position);

                if (dist <= character.Stats.detectionRange)
                {
                    character.Animator?.SetBool("isMoving", false);
                    _state = State.Alert;
                    return;
                }
            }

            character.Animator?.SetBool("isMoving", true);

            float distToSlot = Vector2.Distance(character.FeetPoint.position, slot);
            if (distToSlot <= 0.05f)
            {
                transform.position = slot;
                character.Animator?.SetBool("isMoving", false);
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z);

                _attackTimer = 0f;
                _isAttacking = false;
                _state       = State.Idle;
                return;
            }

            Vector2 dir = ((Vector2)slot - (Vector2)transform.position).normalized;
            character.Move(dir);
        }

        Hero FindClosestHero()
        {
            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            Hero closest  = null;
            float minDist = float.MaxValue;

            foreach (Hero h in heroes)
            {
                if (!h.IsAlive) continue;
                // usa
                float dist = Vector2.Distance(character.FeetPoint.position, h.FeetPoint.position);

                if (dist < minDist) { minDist = dist; closest = h; }
            }
            return closest;
        }

        void FaceTarget(Vector3 targetPos)
        {
            float dir = targetPos.x - transform.position.x;
            if (dir == 0) return;
            transform.localScale = new Vector3(
                Mathf.Sign(dir) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z);
        }

        IEnumerator AttackRoutine(Hero target)
        {
            _isAttacking = true;
            character.Animator?.SetTrigger("attack");

            // tempo até o impacto = metade do intervalo de ataque
            float impactTime  = (1f / character.Stats.attackSpeed) * 0.4f;
            // tempo restante até liberar o próximo ataque
            float recoveryTime = (1f / character.Stats.attackSpeed) * 0.6f;

            yield return new WaitForSeconds(impactTime);

            if (target != null && target.IsAlive)
                character.Attack(target);

            yield return new WaitForSeconds(recoveryTime);

            _isAttacking = false;
        }

        void OnDrawGizmosSelected()
        {
            if (character == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, character.Stats.detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, character.Stats.attackRange);
        }
    }
}