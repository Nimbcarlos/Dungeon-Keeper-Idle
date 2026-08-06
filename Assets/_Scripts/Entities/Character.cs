using UnityEngine;



public abstract class Character : Entity
{
    public Stats Stats { get; private set; }

    [Header("Combat Points")]
    [SerializeField] private Transform _feetPoint;
    [SerializeField] private Transform _combatPoint;
    [SerializeField] private Transform _headPoint;

    public Transform FeetPoint   => _feetPoint   != null ? _feetPoint   : transform;
    public Transform CombatPoint => _combatPoint  != null ? _combatPoint  : transform;
    public Transform HeadPoint   => _headPoint   != null ? _headPoint   : transform;

    public virtual void Initialize(Stats sourceStats)
    {
        Stats = sourceStats.Clone();
        Health.Initialize(Stats);
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
    }

    public bool InRange(ITargetable target)
    {
        if (target == null) return false;
        return Vector2.Distance(
            transform.position,
            target.Transform.position) <= Stats.attackRange;
    }

    protected override void OnDeath() => Die();

    protected virtual void Die()
    {
        OnDieEffect();
        Destroy(gameObject, 0.1f);
    }

    protected virtual void OnDieEffect() { }
    protected virtual void OnAttack()    { }
    protected virtual void OnHit()       { }
}