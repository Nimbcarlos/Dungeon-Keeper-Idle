using UnityEngine;

public enum ProjectileBehavior
{
    Straight,
    Piercing,
    Homing,
    Volley,
    Bounce,
    ArcShot
}

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileData _data;
    
    private Vector2    _direction;
    private GameObject _owner;
    private ITargetable _homingTarget;
    private int        _bounceCount;
    private SpriteRenderer _sr;
    private Animator   _anim;
    private Transform  _visualTransform;

    // Propriedades simplificadas
    private float Speed => _data != null ? _data.speed : 8f;
    private int   Damage => _data != null ? _data.damage : 10;
    private float Lifetime => _data != null ? _data.lifetime : 5f;
    private float VisualOffset => _data != null ? _data.visualRotationOffset : 0f;

    // Variáveis para ArcShot
    private float _arcTimer;
    private Vector3 _arcOrigin;
    private Vector3 _arcTarget;
    private float _arcDuration;
    private float _arcHeight = 3f;
    private bool _isArcing = false;
// campos de override — aplicados pelos upgrades
    private ProjectileBehavior _behaviorOverride;
    private int   _damageOverride;
    private float _speedOverride;
    private int   _bounceOverride;
    private int   _bounceBonus;

    public void Initialize(Vector2 direction, GameObject owner,
                        ProjectileData data,
                        ProjectileBehavior behaviorOverride,
                        int damageBonus   = 0,
                        float speedBonus  = 0f,
                        int bounceBonus   = 0,
                        ITargetable homingTarget = null)
    {
        if (data != null) _data = data;

        _direction    = direction.normalized;
        _owner        = owner;
        _homingTarget = homingTarget;

        // aplica override de behavior
        _behaviorOverride = behaviorOverride;

        // aplica bônus sem modificar o ScriptableObject
        _damageOverride = data.damage + damageBonus;
        _speedOverride  = data.speed  + speedBonus;
        _bounceOverride = data.maxBounce + bounceBonus;

        SetupVisuals();
        UpdateVisualOrientation();
        Destroy(gameObject, data.lifetime);
    }

    // Inicialização específica para ArcShot
    public void InitializeArc(Vector3 target, float height, GameObject owner, ProjectileData data = null)
    {
        if (data != null) _data = data;
        _owner = owner;
        _arcOrigin = transform.position;
        _arcTarget = target;
        _arcHeight = height;
        _isArcing = true;
        _arcTimer = 0f;

        // Duração baseada na distância e velocidade
        float distance = Vector3.Distance(_arcOrigin, _arcTarget);
        _arcDuration = distance / Speed;

        SetupVisuals();
        Destroy(gameObject, _arcDuration + 0.1f);
    }

    private void SetupVisuals()
    {
        if (_data == null) return;

        if (_sr != null)
        {
            if (_data.sprite != null) _sr.sprite = _data.sprite;
            _sr.color = _data.tint;
        }

        if (_anim != null && _data.animatorController != null)
        {
            _anim.runtimeAnimatorController = _data.animatorController;
        }

        if (_data.trailVFX != null)
        {
            Instantiate(_data.trailVFX, _visualTransform != null ? _visualTransform : transform);
        }
    }

    void Awake()
    {
        _visualTransform = transform.Find("Visual");
        if (_visualTransform == null) _visualTransform = transform;

        _sr = _visualTransform.GetComponent<SpriteRenderer>();
        _anim = _visualTransform.GetComponent<Animator>();
    }

    void Update()
    {
        if (_isArcing)
        {
            MoveArc();
            return;
        }

        ProjectileBehavior currentBehavior = _data != null ? _data.behavior : ProjectileBehavior.Straight;

        switch (currentBehavior)
        {
            case ProjectileBehavior.Straight:
            case ProjectileBehavior.Piercing:
            case ProjectileBehavior.Bounce:
            case ProjectileBehavior.Volley: // Volley se move como Straight, a lógica de "leque" é no disparo
                MoveStraight();
                break;
            case ProjectileBehavior.Homing:
                MoveHoming();
                break;
        }
    }

    private void MoveStraight()
    {
        transform.position += (Vector3)(_direction * Speed * Time.deltaTime);
    }

    private void MoveHoming()
    {
        if (_homingTarget == null || !_homingTarget.IsAlive)
        {
            MoveStraight();
            return;
        }

        float strength = _data != null ? _data.homingStrength : 3f;
        Vector2 targetDir = ((Vector2)_homingTarget.Transform.position - (Vector2)transform.position).normalized;

        _direction = Vector2.Lerp(_direction, targetDir, strength * Time.deltaTime).normalized;

        transform.position += (Vector3)(_direction * Speed * Time.deltaTime);
        UpdateVisualOrientation();
    }

    private void MoveArc()
    {
        _arcTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_arcTimer / _arcDuration);

        Vector3 lastPos = transform.position;
        
        // Interpolação linear no plano XZ (ou XY para 2D) + Seno para a altura (Y)
        Vector3 currentPos = Vector3.Lerp(_arcOrigin, _arcTarget, t);
        float heightOffset = Mathf.Sin(t * Mathf.PI) * _arcHeight;
        currentPos.y += heightOffset;

        transform.position = currentPos;

        // Atualiza a direção visual baseada no movimento real do arco
        _direction = (currentPos - lastPos).normalized;
        UpdateVisualOrientation();

        if (t >= 1f)
        {
            OnArcEnd();
        }
    }

    private void OnArcEnd()
    {
        // Pode gerar um efeito de explosão ao cair
        if (_data != null && _data.impactVFX != null)
        {
            Instantiate(_data.impactVFX, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void UpdateVisualOrientation()
    {
        if (_data != null && !_data.rotateSprite) return;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        
        if (_visualTransform != null)
        {
            _visualTransform.rotation = Quaternion.Euler(0, 0, angle + VisualOffset);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"Projétil colidiu com: {other.name} | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (other.gameObject == _owner) return;

        bool isWall   = other.gameObject.layer == LayerMask.NameToLayer("Wall");
        bool isTarget = other.GetComponent<IDamageable>() != null;

        if (!isWall && !isTarget) return;

        ProjectileBehavior currentBehavior = _data != null
            ? _data.behavior : ProjectileBehavior.Straight;

        // aplica dano se não for parede
        if (isTarget && !isWall)
        {
            IDamageable target = other.GetComponent<IDamageable>();
            target.TakeDamage(Damage);
            // Debug.Log($"Projétil acertou {other.name} por {Damage} de dano.");

            if (_data != null && _data.impactVFX != null)
                Instantiate(_data.impactVFX, transform.position, Quaternion.identity);
        }

        switch (currentBehavior)
        {
            case ProjectileBehavior.Piercing:
                // não destrói — continua atravessando, nunca reflete
                break;

            case ProjectileBehavior.Bounce:
                _bounceCount++;
                int maxBounce = _data != null ? _data.maxBounce + _bounceBonus : 2;

                if (_bounceCount >= maxBounce)
                {
                    Destroy(gameObject);
                    return;
                }
                // Debug.Log($"Projétil ricocheteou! ({_bounceCount}/{maxBounce})");
                // calcula normal da superfície atingida
                Vector2 normal = GetCollisionNormal(other);
                _direction = Vector2.Reflect(_direction, normal).normalized;
                UpdateVisualOrientation();

                // busca próximo alvo na direção refletida
                // (o projétil vai naturalmente na nova direção)
                break;

            default:
                if (_data == null || _data.destroyOnHit)
                    Destroy(gameObject);
                break;
        }
    }

    Vector2 GetCollisionNormal(Collider2D other)
    {
        bool isWall = other.gameObject.layer == LayerMask.NameToLayer("Wall");

        if (isWall)
        {
            // usa o bounds do collider para determinar orientação da parede
            Bounds bounds = other.bounds;
            bool isHorizontalWall = bounds.size.x > bounds.size.y;

            if (isHorizontalWall)
                // parede horizontal (cima/baixo) — inverte Y
                return new Vector2(0, -Mathf.Sign(_direction.y));
            else
                // parede vertical (esquerda/direita) — inverte X
                return new Vector2(-Mathf.Sign(_direction.x), 0);
        }
        else
        {
            Vector2 toOther = ((Vector2)other.transform.position
                - (Vector2)transform.position).normalized;

            if (Mathf.Abs(toOther.x) > Mathf.Abs(toOther.y))
                return new Vector2(-Mathf.Sign(toOther.x), 0);
            else
                return new Vector2(0, -Mathf.Sign(toOther.y));
        }
    }

}
