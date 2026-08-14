using UnityEngine;

namespace DungeonKeeper
{
    public class HeroBrain : CharacterBrain
    {
        public enum BrainState { Spawning, MovingToTarget, InCombat, SackingTreasure }

        [Header("Configurações do Cérebro")]
        [SerializeField] private float _postCombatPauseDuration = 0.35f;

        private BrainState _currentState = BrainState.Spawning;
        private Hero       _hero;
        private float      _attackTimer;
        private float      _pauseTimer;
        private Vector2    _spawnPoint;

        private const float Y_THRESHOLD         = 0.12f; // Aumentado levemente para evitar "jitter"
        private const float SPAWN_SAFE_X_OFFSET = 1.5f;

        private Vector2 MyFeetPos => character.FeetPoint != null ? (Vector2)character.FeetPoint.position : (Vector2)transform.position;

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
            Monster closestMonster = FindClosestMonster();

            if (closestMonster != null)
            {
                Vector2 targetFeetPos = GetEntityFeetPos(closestMonster);

                if (IsTargetInAttackRange(targetFeetPos))
                {
                    SetState(BrainState.InCombat);
                    return;
                }

                Vector2 moveDir = CalculateTwoPhaseMovement(MyFeetPos, targetFeetPos);
                float lookDir = moveDir.x != 0 ? moveDir.x : (targetFeetPos.x - MyFeetPos.x);

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
            Monster closestMonster = FindClosestMonster();

            if (closestMonster == null || !closestMonster.IsAlive)
            {
                _pauseTimer = _postCombatPauseDuration;
                SetState(BrainState.MovingToTarget);
                return;
            }

            Vector2 targetFeetPos = GetEntityFeetPos(closestMonster);

            if (!IsTargetInAttackRange(targetFeetPos))
            {
                SetState(BrainState.MovingToTarget);
                return;
            }

            // CORREÇÃO: Removemos o ForceAlignY. 
            // Uma vez em combate, o herói para totalmente para evitar o deslizamento vertical.
            ExecuteMovement(Vector2.zero, targetFeetPos.x - MyFeetPos.x);

            if (_attackTimer <= 0f)
            {
                if (_hero != null) _hero.SetState(0);
                character.Animator?.SetTrigger("Slash");

                character.Attack(closestMonster);

                if (!closestMonster.IsAlive)
                {
                    _pauseTimer = _postCombatPauseDuration;
                    SetState(BrainState.MovingToTarget);
                }

                _attackTimer = 1f / character.Stats.attackSpeed;
            }
        }

        private void ExecuteSackTreasure(Treasure treasure, float treasureY)
        {
            ExecuteMovement(Vector2.zero, 1f);
            // Alinhamento final apenas no tesouro, usando MoveTowards controlado
            Vector3 pos = transform.position;
            pos.y = treasureY;
            transform.position = pos;
            
            this.enabled = false;

            if (_hero != null && _hero.Data != null)
                treasure.Sack(_hero.Data.goldReward);

            if (_hero != null)
                _hero.CelebrateVictoryAndDespawn(1.5f);
        }

        public void SetState(BrainState newState) => _currentState = newState;

        private void ExecuteMovement(Vector2 moveDirection, float lookDirectionX)
        {
            ApplyVisualFlip(lookDirectionX);

            if (moveDirection == Vector2.zero || IsBlockedByAlly())
            {
                if (_hero != null) _hero.SetState(0);
                character.Move(Vector2.zero); // Garante que a velocidade seja zerada
                return;
            }

            if (_hero != null) _hero.SetState(2);
            character.Move(moveDirection);
        }

        private Vector2 CalculateTwoPhaseMovement(Vector2 currentPos, Vector2 targetPos)
        {
            float deltaY = targetPos.y - currentPos.y;
            float deltaX = targetPos.x - currentPos.x;

            // Prioriza alinhar a Lane (Y) antes de avançar (X)
            if (Mathf.Abs(deltaY) > Y_THRESHOLD)
                return new Vector2(0f, Mathf.Sign(deltaY));

            if (Mathf.Abs(deltaX) > 0.05f)
                return new Vector2(Mathf.Sign(deltaX), 0f);

            return Vector2.zero;
        }

        private bool IsTargetInAttackRange(Vector2 targetFeetPos)
        {
            float diffX = Mathf.Abs(MyFeetPos.x - targetFeetPos.x);
            float diffY = Mathf.Abs(MyFeetPos.y - targetFeetPos.y);

            // Considera em range se estiver perto no X e na mesma lane (Y)
            return diffX <= character.Stats.attackRange && diffY <= Y_THRESHOLD;
        }

        private Monster FindClosestMonster()
        {
            Monster[] monsters = FindObjectsByType<Monster>(FindObjectsInactive.Exclude);
            Monster closest = null;
            float minDist = float.MaxValue;

            foreach (Monster m in monsters)
            {
                if (m == null || !m.IsAlive) continue;
                Vector2 mFeet = GetEntityFeetPos(m);
                float dist = Vector2.Distance(MyFeetPos, mFeet);
                if (dist < minDist) { minDist = dist; closest = m; }
            }
            return closest;
        }

        private void ApplyVisualFlip(float directionX)
        {
            if (Mathf.Abs(directionX) < 0.01f) return;
            Vector3 scale = transform.localScale;
            scale.x = directionX < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        private bool IsBlockedByAlly()
        {
            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            foreach (Hero h in heroes)
            {
                if (h == character || h == null || !h.IsAlive) continue;
                Vector2 otherFeetPos = GetEntityFeetPos(h);
                if (otherFeetPos.x > MyFeetPos.x && Vector2.Distance(MyFeetPos, otherFeetPos) < 0.8f)
                    return true;
            }
            return false;
        }

        private Vector2 GetEntityFeetPos(Character entity)
        {
            return entity.FeetPoint != null ? (Vector2)entity.FeetPoint.position : (Vector2)entity.transform.position;
        }
    }
}
