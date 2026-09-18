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

        private const float Y_THRESHOLD = 0.5f;

        private bool IsRanged => _monster != null && _monster.EffectiveAttackType == AttackType.Ranged;

        protected override void Awake()
        {
            base.Awake();
            _monster = GetComponent<Monster>();
        }

        protected override void Think()
        {
            if (_isAttacking || _monster == null || !_monster.IsAlive) return;

            // 🎯 O radar agora se baseia no novo tipo genérico!
            Hero closest = FindClosestHeroInSight();

            switch (_state)
            {
                case BrainState.Idle:      HandleIdle(closest);      break;
                case BrainState.Alert:     HandleAlert(closest);     break;
                case BrainState.Combat:    HandleCombat(closest);    break;
                case BrainState.Returning: HandleReturning(closest); break;
            }
        }

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

            if (IsTargetInAttackRange(closest))
            {
                SetState(BrainState.Combat);
                return;
            }

            if (IsRanged)
            {
                SetState(BrainState.Returning);
                HandleReturning(closest);
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

            if (!IsTargetInAttackRange(closest))
            {
                StopAttack();
                SetState(BrainState.Alert);
                return;
            }

            ExecuteMovement(Vector2.zero, targetFeetPos.x - MyFeetPos.x);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f && !_isAttacking)
            {
                var melee = GetComponent<MeleeSkill>();
                _attackTimer = _monster.Data != null && _monster.EffectiveAttackType == AttackType.Melee && _monster.ActiveMeleeData != null && melee != null
                    ? melee.AttackInterval : 1f / Mathf.Max(0.1f, character.Stats.attackSpeed);
                
                if (_monster.Data != null && _monster.EffectiveAttackType == AttackType.Ranged)
                {
                    var ranged = GetComponent<ProjectileSkill>();
                    if (ranged != null) _attackTimer = ranged.AttackInterval;
                }
                
                StartCoroutine(AttackRoutine(closest));
            }
        }

        private void HandleReturning(Hero closest)
        {
            if (closest != null && IsInDetectionRange(closest) &&
                (!IsRanged || IsTargetInAttackRange(closest)))
            {
                SetState(BrainState.Alert);
                return;
            }

            Vector2 slotPos = _monster.GuardPosition;

            if (Vector2.Distance((Vector2)transform.position, slotPos) <= 0.05f)
            {
                transform.position = slotPos;
                ExecuteMovement(Vector2.zero, 1f);
                SetState(BrainState.Idle);
                return;
            }

            Vector2 moveDir = CalculateTwoPhaseMovement(transform.position, slotPos);
            float lookDir = moveDir.x != 0 ? moveDir.x : (slotPos.x - MyFeetPos.x);

            ExecuteMovement(moveDir, lookDir);
        }

        public void SetState(BrainState newState)
        {
            _state = newState;
        }

        private void StopAttack()
        {
            StopAllCoroutines();
            _isAttacking = false;
            _attackTimer = 0f;
        }

        private bool IsInDetectionRange(Hero hero)
        {
            if (hero == null || !hero.IsAlive) return false;
            return Vector2.Distance(MyFeetPos, GetEntityFeetPos(hero)) <= character.Stats.detectionRange;
        }

        private bool IsTargetInAttackRange(Character target)
        {
            if (target == null) return false;
            
            if (IsRanged)
            {
                Vector2 delta = target.CombatPoint.position - character.CombatPoint.position;
                return delta.sqrMagnitude <= character.Stats.attackRange * character.Stats.attackRange;
            }

            Vector2 targetFeetPos = GetEntityFeetPos(target);
            float diffX = Mathf.Abs(MyFeetPos.x - targetFeetPos.x);
            float diffY = Mathf.Abs(MyFeetPos.y - targetFeetPos.y);
            return diffX <= character.Stats.attackRange && diffY <= Y_THRESHOLD;
        }

        private Hero FindClosestHeroInSight()
        {
            // 🎯 Filtro Otimizado usando o radar e a herança
            Hero[] aliveHeroes = GetAliveEntities<Hero>();
            
            Hero closest = null;
            float minDist = float.MaxValue;

            foreach (Hero h in aliveHeroes)
            {
                float dist = IsRanged 
                    ? Vector2.Distance(character.CombatPoint.position, h.CombatPoint.position) 
                    : Vector2.Distance(MyFeetPos, GetEntityFeetPos(h));

                if (dist <= character.Stats.detectionRange && dist < minDist)
                {
                    minDist = dist;
                    closest = h;
                }
            }
            return closest;
        }

        private IEnumerator AttackRoutine(Hero target)
        {
            _isAttacking = true;
            character.Animator?.SetTrigger("attack");

            float totalDuration = 1f / Mathf.Max(0.1f, character.Stats.attackSpeed);
            float impactTime    = totalDuration * 0.4f;
            float recoveryTime  = totalDuration * 0.6f;

            yield return new WaitForSeconds(impactTime);

            if (target != null && target.IsAlive)
                character.Attack(target);

            yield return new WaitForSeconds(recoveryTime);
            _isAttacking = false;
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