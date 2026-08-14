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
                // 1. Instancia exatamente na posição do HeadPoint (em cima da cabeça)
                GameObject bar = Instantiate(_healthBarPrefab, HeadPoint.position, Quaternion.identity);
                _healthBar = bar.GetComponent<HealthBar>();
                
                // 2. Passa a referência do HeadPoint para a barra seguir o ponto correto!
                _healthBar.Initialize(HeadPoint);
                _healthBar.UpdateHealth(Health.MaxHP);
            }
        }

        public virtual void Attack(IDamageable target)
        {
            if (target == null) return;
            target.TakeDamage(Stats.attackPower);
            OnAttack();
        }

        /// <summary>
        /// Move o personagem no Espaço do Mundo (Space.World) para evitar que o localScale (Flip) inverta a direção do movimento!
        /// </summary>
        public virtual void Move(Vector2 direction)
        {
            // CRÍTICO: Space.World garante que Vector2.right SEMPRE vá para a direita do mapa, independente do Flip!
            transform.Translate(direction * Stats.moveSpeed * Time.deltaTime, Space.World);
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

        /// <summary>
        /// Valida o alcance usando o FeetPoint para garantir precisão com os pivôs do chão
        /// </summary>
        public bool InRange(ITargetable target)
        {
            if (target == null) return false;
            
            Vector2 myPos = FeetPoint != null ? (Vector2)FeetPoint.position : (Vector2)transform.position;
            Vector2 targetPos = target.Transform != null ? (Vector2)target.Transform.position : (Vector2)transform.position;

            return Vector2.Distance(myPos, targetPos) <= Stats.attackRange;
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