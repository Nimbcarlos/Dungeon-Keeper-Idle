using UnityEngine;

namespace DungeonKeeper
{
    public class HeroBrain : CharacterBrain
    {
        public enum BrainState { Spawning, MovingToTarget, InCombat, SackingTreasure }

        [Header("Configurações do Cérebro")]
        [SerializeField] private float _postCombatPauseDuration = 0.35f;

        [Header("Módulos de Comportamento")]
        [SerializeField] private TargetSelectionSO _targetStrategy;

        private BrainState _currentState = BrainState.Spawning;
        private Hero       _hero;
        private float      _attackTimer;
        private float      _pauseTimer;
        private Vector2    _spawnPoint;

        private const float SPAWN_SAFE_X_OFFSET = 1.5f;

        private HeroRoleStrategy _activeStrategy;
        public HeroRoleStrategy ActiveStrategy 
        { 
            get 
            {
                if (_activeStrategy == null) InitializeStrategy();
                return _activeStrategy;
            }
        }

        public Character BaseCharacter => character;
        public TargetSelectionSO TargetStrategy => _targetStrategy;

        protected override void Awake()
        {
            base.Awake();
            _hero = GetComponent<Hero>();
        }

        private void Start()
        {
            _spawnPoint = MyFeetPos;
            SetState(BrainState.Spawning);
        }

        private void InitializeStrategy()
        {
            if (_hero != null && _hero.Data != null)
            {
                switch (_hero.Data.attackType)
                {
                    case HeroAttackType.Melee:
                        _activeStrategy = new MeleeRoleStrategy();
                        break;
                    case HeroAttackType.Ranged:
                        _activeStrategy = new RangedRoleStrategy();
                        break;
                    case HeroAttackType.Mage:
                    case HeroAttackType.Healer:
                        _activeStrategy = new RangedRoleStrategy(); 
                        break;
                    default:
                        _activeStrategy = new MeleeRoleStrategy();
                        break;
                }
            }
            else
            {
                _activeStrategy = new MeleeRoleStrategy();
            }

            _activeStrategy.Initialize(this);
        }

        protected override void Think()
        {
            if (_hero == null || !_hero.IsAlive) return;

            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                ExecuteMovement(Vector2.zero, 0f);
                return;
            }

            _attackTimer -= Time.deltaTime;

            switch (_currentState)
            {
                case BrainState.Spawning:       UpdateSpawningState(); break;
                case BrainState.MovingToTarget: UpdateMovingState();   break;
                case BrainState.InCombat:       UpdateCombatState();   break;
                case BrainState.SackingTreasure: break;
            }
        }

        private void UpdateSpawningState()
        {
            if ((MyFeetPos.x - _spawnPoint.x) < SPAWN_SAFE_X_OFFSET)
            {
                ExecuteMovement(Vector2.right, 1f);
            }
            else
            {
                SetState(BrainState.MovingToTarget);
            }
        }

        private void UpdateMovingState()
        {
            Character targetEntity = ActiveStrategy.FindTarget();

            if (targetEntity != null)
            {
                // 🐛 A ARMADILHA PARA O ALVO FANTASMA
                Debug.Log($"🎯 [{name}] Target={targetEntity.name} | Alive={targetEntity.IsAlive} | MyPos={MyFeetPos} | TargetPos={ActiveStrategy.GetTargetPosition(targetEntity)}");

                Vector2 targetPos = ActiveStrategy.GetTargetPosition(targetEntity);

                if (IsTargetInAttackRange(targetEntity, targetPos))
                {
                    SetState(BrainState.InCombat);
                    return;
                }

                Vector2 moveDir = CalculateTwoPhaseMovement(MyFeetPos, targetPos);
                float lookDir = moveDir.x != 0 ? moveDir.x : (targetPos.x - MyFeetPos.x);

                ExecuteMovement(moveDir, lookDir);
                return;
            }

            Treasure treasure = FindAnyObjectByType<Treasure>();
            if (treasure == null)
            {
                ExecuteMovement(Vector2.right, 1f);
                return;
            }

            Vector2 treasureFeetPos = treasure.transform.position;
            Vector2 targetSackPos = new Vector2(treasureFeetPos.x - 0.8f, treasureFeetPos.y);

            if ((targetSackPos.x - MyFeetPos.x) > 0.05f)
            {
                Vector2 moveDir = CalculateTwoPhaseMovement(MyFeetPos, targetSackPos);
                ExecuteMovement(moveDir, 1f);
            }
            else
            {
                SetState(BrainState.SackingTreasure);
                ExecuteSackTreasure(treasure, treasureFeetPos.y);
            }
        }

        private void UpdateCombatState()
        {
            Character targetEntity = ActiveStrategy.FindTarget();

            if (targetEntity == null || !targetEntity.IsAlive)
            {
                _attackTimer = 0f; 
                _pauseTimer = _postCombatPauseDuration;
                SetState(BrainState.MovingToTarget);
                return;
            }

            Vector2 targetPos = ActiveStrategy.GetTargetPosition(targetEntity);

            if (!IsTargetInAttackRange(targetEntity, targetPos))
            {
                SetState(BrainState.MovingToTarget);
                return;
            }

            ExecuteMovement(Vector2.zero, GetEntityFeetPos(targetEntity).x - MyFeetPos.x);

            if (_attackTimer <= 0f)
            {
                if (_hero != null) _hero.SetState(0);
                
                ActiveStrategy.ExecuteAction(targetEntity);

                if (!targetEntity.IsAlive)
                {
                    _attackTimer = 0f; 
                    _pauseTimer = _postCombatPauseDuration;
                    SetState(BrainState.MovingToTarget);
                }
                else
                {
                    _attackTimer = 1f / character.Stats.attackSpeed;
                }
            }
        }

        public bool IsTargetInAttackRange(Character target, Vector2 targetAttackPos)
        {
            if (target == null || !target.IsAlive)
                return false;

            Vector2 targetFeetPos = GetEntityFeetPos(target);
            
            float toleranceX = (_currentState == BrainState.InCombat) ? 0.8f : 0.25f;
            float rangeBonus = (_currentState == BrainState.InCombat) ? 0.6f : 0.0f;

            float diffX = Mathf.Abs(MyFeetPos.x - targetAttackPos.x);
            float diffY = Mathf.Abs(MyFeetPos.y - targetAttackPos.y);

            const float yThreshold = 0.5f;
            bool closeToSlot = diffX <= toleranceX && diffY <= (yThreshold + toleranceX);

            float distToTarget = Vector2.Distance(MyFeetPos, targetFeetPos);
            bool closeToTarget = distToTarget <= (character.Stats.attackRange + 0.3f + rangeBonus);

            return closeToSlot || closeToTarget;
        }

        private bool IsBlockedByAlly()
        {
            if (_currentState != BrainState.MovingToTarget)
                return false;

            Character targetEntity = ActiveStrategy.FindTarget();

            if (targetEntity != null)
            {
                float distToTarget = Vector2.Distance(MyFeetPos, GetEntityFeetPos(targetEntity));
                if (distToTarget < 1.5f)
                    return false;
            }
            else
            {
                Treasure treasure = FindAnyObjectByType<Treasure>();
                if (treasure != null)
                {
                    float distToTreasure = Vector2.Distance(MyFeetPos, treasure.transform.position);
                    if (distToTreasure < 2.0f)
                        return false;
                }
            }

            // 🎯 Utiliza a busca rápida herdada da classe base
            Hero[] heroes = GetAliveEntities<Hero>();
            foreach (Hero h in heroes)
            {
                if (h == character)
                    continue;

                HeroBrain otherBrain = h.GetComponent<HeroBrain>();
                
                if (otherBrain != null && 
                   (otherBrain._currentState == BrainState.InCombat || otherBrain._currentState == BrainState.SackingTreasure))
                    continue;

                Vector2 otherFeetPos = GetEntityFeetPos(h);
                float deltaX = otherFeetPos.x - MyFeetPos.x;
                float deltaY = Mathf.Abs(otherFeetPos.y - MyFeetPos.y);

                if (deltaX > 0.15f && deltaX < 0.6f && deltaY < 0.5f)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetState(BrainState newState)
        {
            if (_currentState == newState)
                return;

            _currentState = newState;

            // 🎯 O FIX PREVENTIVO: Garante a intenção física assim que o estado muda
            switch (newState)
            {
                case BrainState.InCombat:
                case BrainState.SackingTreasure:
                    ExecuteMovement(Vector2.zero, 0f);
                    break;
            }
        }

        // Sobrescreve o movimento base para injetar a lógica de bloqueio de aliados e animações específicas do Herói
        protected override void ExecuteMovement(Vector2 moveDirection, float lookDirectionX)
        {
            ApplyVisualFlip(lookDirectionX);

            bool isBlocked = IsBlockedByAlly();

            // 🐛 DEBUG TEMPORÁRIO DE MOVIMENTO
            Debug.Log($"🚶 [{name}] State={_currentState} | Move={moveDirection} | Blocked={isBlocked} | HeroAnim={(moveDirection == Vector2.zero || isBlocked ? "STAND" : "RUN")}");

            if (moveDirection == Vector2.zero || isBlocked)
            {
                if (_hero != null) _hero.SetState(0); // 0 = Idle/Stand
                character.Move(Vector2.zero);
                return;
            }

            if (_hero != null) _hero.SetState(2); // 2 = Run
            character.Move(moveDirection);
        }
 
        private void ExecuteSackTreasure(Treasure treasure, float treasureY)
        {
            ExecuteMovement(Vector2.zero, 1f);
            Vector3 pos = transform.position;
            pos.y = treasureY;
            transform.position = pos;
            
            this.enabled = false;

            if (_hero != null && _hero.Data != null)
                treasure.Sack(_hero.Data.goldReward);

            if (_hero != null)
                _hero.CelebrateVictoryAndDespawn(1.5f);
        }
    }
}