using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DungeonKeeper
{
    public abstract class Character : Entity
    {
        public Stats Stats { get; private set; }

        [Header("Componentes Auxiliares")]
        public CharacterMovement Movement { get; private set; }
        public StatusEffectManager StatusEffects { get; private set; }

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

        protected override void Awake()
        {
            base.Awake();
            Movement = GetComponent<CharacterMovement>();
            StatusEffects = GetComponent<StatusEffectManager>();
        }

        public virtual void Initialize(Stats sourceStats)
        {
            Stats = sourceStats.Clone();
            Health.Initialize(Stats);
            
            if (_healthBarPrefab != null)
            {
                GameObject bar = Instantiate(_healthBarPrefab, HeadPoint.position, Quaternion.identity);
                _healthBar = bar.GetComponent<HealthBar>();
                _healthBar.Initialize(HeadPoint);
                _healthBar.UpdateHealth(Health.MaxHP);
            }
        }

        public override void TakeDamage(int amount)
        {
            Health.TakeDamage(amount);
            OnHit();
            DamageTextManager.Instance?.SpawnDamageText(HeadPoint.position, amount);
            _healthBar?.UpdateHealth(Health.Percent);
        }

        public virtual void Move(Vector2 direction) => Movement?.Move(direction);

        public bool HasStatusEffect(BehaviorType behavior) => StatusEffects != null && StatusEffects.HasEffect(behavior);

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

        public virtual void Attack(IDamageable target)
        {
            if (target == null) return;
            target.TakeDamage(Stats.attackPower);
            OnAttack();
        }
    }
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        private List<BehaviorType> _activeStatusEffects = new List<BehaviorType>();

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

        public bool HasStatusEffect(BehaviorType behavior)
        {
            return _activeStatusEffects.Contains(behavior);
        }

        public void ApplyPoison(float damage, float duration)
        {
            StartCoroutine(PoisonRoutine(damage, duration));
        }

        private IEnumerator PoisonRoutine(float damagePerSecond, float duration)
        {
            if (!_activeStatusEffects.Contains(BehaviorType.StatusPoison))
                _activeStatusEffects.Add(BehaviorType.StatusPoison);

            float elapsed = 0f;

            while (elapsed < duration && IsAlive)
            {
                yield return new WaitForSeconds(1f);
                TakeDamage(Mathf.RoundToInt(damagePerSecond));
                elapsed += 1f;
            }

            _activeStatusEffects.Remove(BehaviorType.StatusPoison);
        }

        public void ApplyBurn(float damage, float duration)
        {
            StartCoroutine(BurnRoutine(damage, duration));
        }

        private IEnumerator BurnRoutine(float damagePerSecond, float duration)
        {
            if (!_activeStatusEffects.Contains(BehaviorType.StatusBurn))
                _activeStatusEffects.Add(BehaviorType.StatusBurn);

            float elapsed = 0f;

            while (elapsed < duration && IsAlive)
            {
                yield return new WaitForSeconds(0.5f);
                TakeDamage(Mathf.RoundToInt(damagePerSecond / 2f));
                elapsed += 0.5f;
            }

            _activeStatusEffects.Remove(BehaviorType.StatusBurn);
        }
    }
}
*/