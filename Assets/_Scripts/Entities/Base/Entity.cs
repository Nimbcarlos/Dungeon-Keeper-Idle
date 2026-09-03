using UnityEngine;

namespace DungeonKeeper
{
    [RequireComponent(typeof(Health))]
    public abstract class Entity : MonoBehaviour, ITargetable, IDamageable
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _primarySpriteRenderer;

        // Guarda TODOS os renderizadores do modelo (seja 1 ou 15 partes de corpo)
        private SpriteRenderer[] _allSpriteRenderers;

        public Animator Animator => _animator != null ? _animator : _animator = GetComponentInChildren<Animator>();
        
        // Retorna o renderer principal (ex: torso/corpo) para consultas pontuais
        public SpriteRenderer SpriteRenderer => _primarySpriteRenderer != null ? _primarySpriteRenderer : _primarySpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Propriedade para acessar TODAS as partes visuais
        public SpriteRenderer[] AllSpriteRenderers 
        {
            get
            {
                if (_allSpriteRenderers == null || _allSpriteRenderers.Length == 0)
                    _allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
                return _allSpriteRenderers;
            }
        }

        public Health Health { get; private set; }
        public Transform Transform => transform;
        public bool IsAlive => Health != null && !Health.IsDead;

        protected virtual void Awake()
        {
            Health = GetComponent<Health>();

            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            
            // Busca todas as partes do corpo na hierarquia
            _allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            if (_primarySpriteRenderer == null && _allSpriteRenderers.Length > 0)
            {
                _primarySpriteRenderer = _allSpriteRenderers[0];
            }
        }

        protected virtual void OnEnable()
        {
            if (Health != null) Health.OnDeath += OnDeath;
        }

        protected virtual void OnDisable()
        {
            if (Health != null) Health.OnDeath -= OnDeath;
        }

        /// <summary>
        /// Aplica uma cor (ex: Vermelho no dano) em TODAS as partes do corpo simultaneamente.
        /// </summary>
        public void SetModelColor(Color color)
        {
            foreach (var sprite in AllSpriteRenderers)
            {
                if (sprite != null) sprite.color = color;
            }
        }

        public virtual void TakeDamage(int amount) { }

        protected abstract void OnDeath();
    }
}