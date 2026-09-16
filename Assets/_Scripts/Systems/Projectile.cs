using UnityEngine;

namespace DungeonKeeper
{
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
        private float Speed => _initialized ? _speedOverride : (_data != null ? _data.speed : 8f);
        private int   Damage => _initialized ? _damageOverride : (_data != null ? _data.damage : 10);
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
        private bool _initialized;
        private bool _spent;
        private readonly System.Collections.Generic.HashSet<Character> _hitCharacters =
            new System.Collections.Generic.HashSet<Character>();

        public void Initialize(Vector2 direction, GameObject owner,
                            ProjectileData data,
                            ProjectileBehavior behaviorOverride,
                            int damageBonus   = 0,
                            float speedBonus  = 0f,
                            int bounceBonus   = 0,
                            ITargetable homingTarget = null)
        {
            if (data != null) _data = data;
            if (_data == null) { FinishProjectile(); return; }
            _initialized = true;
            _spent = false;
            _isArcing = false;
            _bounceCount = 0;
            _hitCharacters.Clear();

            _direction    = direction.normalized;
            _owner        = owner;
            _homingTarget = homingTarget;

            // aplica override de behavior
            _behaviorOverride = behaviorOverride;

            // aplica bônus sem modificar o ScriptableObject
            _damageOverride = Mathf.Max(0, _data.damage + damageBonus);
            _speedOverride  = Mathf.Max(0f, _data.speed + speedBonus);
            _bounceOverride = Mathf.Max(1, _data.maxBounce + bounceBonus);

            SetupVisuals();
            UpdateVisualOrientation();
            Destroy(gameObject, Mathf.Max(0.01f, _data.lifetime));
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
            if (_visualTransform != null)
                _visualTransform.localScale = new Vector3(_data.scale.x, _data.scale.y, 1f);

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
            if (_spent) return;
            if (_isArcing)
            {
                MoveArc();
                return;
            }

            ProjectileBehavior currentBehavior = _initialized ? _behaviorOverride : (_data != null ? _data.behavior : ProjectileBehavior.Straight);

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
            if (_spent || other == null) return;
            if (_owner != null && (other.gameObject == _owner || other.transform.IsChildOf(_owner.transform))) return;
            bool isWall = other.gameObject.layer == LayerMask.NameToLayer("Wall");
            var targetCharacter = other.GetComponentInParent<Character>();
            var ownerCharacter = _owner != null ? _owner.GetComponent<Character>() : null;
            if (targetCharacter != null)
            {
                if (!targetCharacter.IsAlive || targetCharacter == ownerCharacter ||
                    (ownerCharacter is Monster && targetCharacter is Monster) ||
                    (ownerCharacter is Hero && targetCharacter is Hero)) return;
                if (!_hitCharacters.Add(targetCharacter)) return;
            }
            IDamageable target = targetCharacter != null ? (IDamageable)targetCharacter : other.GetComponentInParent<IDamageable>();
            if (!isWall && target == null) return;
            if (!isWall)
            {
                var monster = ownerCharacter as Monster;
                if (monster != null && targetCharacter != null) monster.OnHitTarget(targetCharacter, Damage);
                else target.TakeDamage(Damage);
            }
            if (_data != null && _data.impactVFX != null)
                Instantiate(_data.impactVFX, transform.position, Quaternion.identity);
            if (_data != null && _data.hitSFX != null)
                AudioSource.PlayClipAtPoint(_data.hitSFX, transform.position);
            var behavior = _initialized ? _behaviorOverride : (_data != null ? _data.behavior : ProjectileBehavior.Straight);
            if (behavior == ProjectileBehavior.Bounce)
            {
                _bounceCount++;
                int limit = _initialized ? _bounceOverride : (_data != null ? _data.maxBounce : 1);
                if (_bounceCount >= limit) { FinishProjectile(); return; }
                _direction = Vector2.Reflect(_direction, GetCollisionNormal(other)).normalized;
                UpdateVisualOrientation();
            }
            else if (isWall || (behavior != ProjectileBehavior.Piercing && (_data == null || _data.destroyOnHit)))
                FinishProjectile();
        }

        private void FinishProjectile()
        {
            if (_spent) return;
            _spent = true;
            foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
            Destroy(gameObject);
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
}
