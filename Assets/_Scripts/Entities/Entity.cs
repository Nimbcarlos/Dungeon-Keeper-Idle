using UnityEngine;

[RequireComponent(typeof(Health))]
public abstract class Entity : MonoBehaviour, ITargetable, IDamageable
{
    [SerializeField] private Animator       _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public Animator       Animator       => _animator;
    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public Health         Health         { get; private set; }

    public Transform Transform => transform;
    public bool      IsAlive   => Health != null && !Health.IsDead;

    protected virtual void Awake()
    {
        Health = GetComponent<Health>();
        if (_animator == null)       _animator       = GetComponent<Animator>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void OnEnable()
    {
        if (Health != null) Health.OnDeath += OnDeath;
    }

    protected virtual void OnDisable()
    {
        if (Health != null) Health.OnDeath -= OnDeath;
    }

    public virtual void TakeDamage(int amount) { }

    protected abstract void OnDeath();
}