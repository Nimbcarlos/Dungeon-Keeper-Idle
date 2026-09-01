using UnityEngine;

namespace DungeonKeeper
{
    public class MeleeSkill : MonoBehaviour
    {
        [SerializeField] private MeleeSkillData _meleeData;

        private Character _character;
        private float _timer;

        // Modificadores dinâmicos vindos da SkillTree/Upgrades
        private int _bonusDamage;
        private float _bonusRange;
        private int _bonusCleave;
        private float _cooldownReduction;

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        public void Initialize(MeleeSkillData data)
        {
            _meleeData = data;
        }

        private void Update()
        {
            if (_meleeData == null) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                float finalCooldown = Mathf.Max(0.2f, _meleeData.cooldown - _cooldownReduction);
                _timer = finalCooldown;
                TryPerformAttack();
            }
        }

        public void ApplyUpgradeBonus(int extraDamage, float extraRange, int extraCleave, float cdr)
        {
            _bonusDamage += extraDamage;
            _bonusRange += extraRange;
            _bonusCleave += extraCleave;
            _cooldownReduction += cdr;
        }

        private void TryPerformAttack()
        {
            ITargetable target = FindClosestTarget();
            if (target == null) return;

            float finalRange = _meleeData.attackRange + _bonusRange;
            float dist = Vector2.Distance(_character.CombatPoint.position, target.Transform.position);

            // Só ataca se estiver dentro do alcance
            if (dist <= finalRange)
            {
                ExecuteMeleeStrike(target);
            }
        }

        private void ExecuteMeleeStrike(ITargetable primaryTarget)
        {
            float finalRange = _meleeData.attackRange + _bonusRange;
            int maxTargets = _meleeData.maxCleaveTargets + _bonusCleave;
            int finalDamage = _meleeData.baseDamage + _bonusDamage + _character.Stats.attackPower;

            // Busca todos os alvos na área de alcance em volta do CombatPoint
            Collider2D[] hits = Physics2D.OverlapCircleAll(_character.CombatPoint.position, finalRange);
            bool isMonster = GetComponent<Monster>() != null;
            int hitCount = 0;

            foreach (var col in hits)
            {
                if (hitCount >= maxTargets) break;

                // Garante que o monstro ataca heróis e vice-versa
                if (isMonster)
                {
                    Hero hero = col.GetComponent<Hero>();
                    if (hero != null && hero.IsAlive)
                    {
                        hero.TakeDamage(finalDamage);
                        ApplyKnockback(hero.transform);
                        hitCount++;
                    }
                }
                else
                {
                    Monster monster = col.GetComponent<Monster>();
                    if (monster != null && monster.IsAlive)
                    {
                        monster.TakeDamage(finalDamage);
                        ApplyKnockback(monster.transform);
                        hitCount++;
                    }
                }
            }
        }

        private void ApplyKnockback(Transform targetTransform)
        {
            if (_meleeData.knockbackForce <= 0) return;
            Vector2 dir = (targetTransform.position - _character.CombatPoint.position).normalized;
            targetTransform.Translate(dir * _meleeData.knockbackForce * 0.1f, Space.World);
        }

        private ITargetable FindClosestTarget()
        {
            bool isMonster = GetComponent<Monster>() != null;
            float minDist = float.MaxValue;
            ITargetable closest = null;

            if (isMonster)
            {
                foreach (Hero h in FindObjectsByType<Hero>(FindObjectsInactive.Exclude))
                {
                    if (!h.IsAlive) continue;
                    float d = Vector2.Distance(_character.CombatPoint.position, h.transform.position);
                    if (d < minDist) { minDist = d; closest = h; }
                }
            }
            return closest;
        }

        private void OnDrawGizmosSelected()
        {
            if (_character != null && _meleeData != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_character.CombatPoint.position, _meleeData.attackRange + _bonusRange);
            }
        }
    }
}