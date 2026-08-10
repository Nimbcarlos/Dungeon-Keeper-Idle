using UnityEngine;

namespace DungeonKeeper
{
    public abstract class Character : Entity
    {
        public Stats Stats { get; private set; }

        [Header("Combat Points")]
        [SerializeField] private Transform _feetPoint;
        [SerializeField] private Transform _combatPoint;
        [SerializeField] private Transform _headPoint;
        
        [Header("UI Prefabs")]
        [SerializeField] private GameObject _healthBarPrefab;
        private HealthBar _healthBar;

        public Transform FeetPoint   => _feetPoint   != null ? _feetPoint   : transform;
        public Transform CombatPoint => _combatPoint  != null ? _combatPoint  : transform;
        public Transform HeadPoint   => _headPoint   != null ? _headPoint   : transform;

        public virtual void Initialize(Stats sourceStats)
        {
            Stats = sourceStats.Clone();
            Health.Initialize(Stats);
            
            if (_healthBarPrefab != null)
            {
                GameObject bar = Instantiate(_healthBarPrefab, transform.position, Quaternion.identity);
                _healthBar = bar.GetComponent<HealthBar>();
                _healthBar.Initialize(transform);
                _healthBar.UpdateHealth(Health.MaxHP);
            }
        }

        public virtual void Attack(IDamageable target)
        {
            if (target == null) return;
            target.TakeDamage(Stats.attackPower);
            OnAttack();
        }

        public virtual void Move(Vector2 direction)
        {
            transform.Translate(direction * Stats.moveSpeed * Time.deltaTime);
        }

        public override void TakeDamage(int amount)
        {
            Health.TakeDamage(amount);
            OnHit();

            DamageTextManager.Instance?.SpawnDamageText(HeadPoint.position, amount);

            // Use a propriedade Percent direto da sua classe Health!
            _healthBar?.UpdateHealth(Health.Percent);
            // Debug.Log($"HealthBar updated. Current health percent: {Health.Percent}");
        }

        public bool InRange(ITargetable target)
        {
            if (target == null) return false;
            return Vector2.Distance(
                transform.position,
                target.Transform.position) <= Stats.attackRange;
        }

        protected override void OnDeath() => Die();

        public virtual void Die()
        {
            OnDieEffect();
            Destroy(gameObject, 0.1f);
        }

        protected virtual void OnDieEffect()
        {
            CleanupHealthBar();
        }

        // Limpeza de memória garantida para a Barra de HP
        private void CleanupHealthBar()
        {
            if (_healthBar != null)
            {
                Destroy(_healthBar.gameObject);
                _healthBar = null;
            }
        }

        protected virtual void OnDestroy()
        {
            CleanupHealthBar();
        }

        protected virtual void OnAttack()   { }
        protected virtual void OnHit()      { }
    }
}