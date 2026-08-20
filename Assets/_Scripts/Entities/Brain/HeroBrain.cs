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

        private const float Y_THRESHOLD         = 0.12f;
        private const float SPAWN_SAFE_X_OFFSET = 1.5f;

        private Vector2 MyFeetPos => character.FeetPoint != null ? (Vector2)character.FeetPoint.position : (Vector2)transform.position;

        private bool IsRangedHero => character.Stats != null && character.Stats.attackRange > 1.5f;

        [Header("Módulos de Comportamento")]
        [SerializeField] private TargetSelectionSO _targetStrategy;

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
            Monster targetMonster = FindTargetMonster();

            if (targetMonster != null)
            {
                Vector2 targetAttackPos = GetCalculatedAttackPosition(targetMonster);

                if (IsTargetInAttackRange(targetMonster, targetAttackPos))
                {
                    SetState(BrainState.InCombat);
                    return;
                }

                Vector2 moveDir = CalculateTwoPhaseMovement(MyFeetPos, targetAttackPos);
                float lookDir = moveDir.x != 0 ? moveDir.x : (targetAttackPos.x - MyFeetPos.x);

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
            Monster targetMonster = FindTargetMonster();

            if (targetMonster == null || !targetMonster.IsAlive)
            {
                _pauseTimer = _postCombatPauseDuration;
                SetState(BrainState.MovingToTarget);
                return;
            }

            Vector2 targetAttackPos = GetCalculatedAttackPosition(targetMonster);

            if (!IsTargetInAttackRange(targetMonster, targetAttackPos))
            {
                SetState(BrainState.MovingToTarget);
                return;
            }

            ExecuteMovement(Vector2.zero, GetEntityFeetPos(targetMonster).x - MyFeetPos.x);

            if (_attackTimer <= 0f)
            {
                if (_hero != null) _hero.SetState(0);
                
                if (IsRangedHero)
                    character.Animator?.SetTrigger("Shoot"); 
                else
                    character.Animator?.SetTrigger("Slash");

                character.Attack(targetMonster);

                if (!targetMonster.IsAlive)
                {
                    _pauseTimer = _postCombatPauseDuration;
                    SetState(BrainState.MovingToTarget);
                }

                _attackTimer = 1f / character.Stats.attackSpeed;
            }
        }

        private Vector2 GetCalculatedAttackPosition(Monster monster)
        {
            Vector2 monsterPos = GetEntityFeetPos(monster);

            if (IsRangedHero)
            {
                float safeDistance = character.Stats.attackRange * 0.8f;
                return new Vector2(monsterPos.x - safeDistance, monsterPos.y);
            }

            int meleeIndex = GetMeleeQueueIndex(monster);

            // 🎯 AUMENTANDO O ESPAÇAMENTO VISUAL (offsets X e Y maiores):
            switch (meleeIndex)
            {
                case 0: // 1º Melee: Frente direta do monstro
                    return new Vector2(monsterPos.x - 0.75f, monsterPos.y);

                case 1: // 2º Melee: Bem mais para CIMA (+0.60 no Y) e um pouco mais recuado no X
                    return new Vector2(monsterPos.x - 0.85f, monsterPos.y + 0.60f);

                case 2: // 3º Melee: Bem mais para BAIXO (-0.60 no Y) e um pouco mais recuado no X
                    return new Vector2(monsterPos.x - 0.85f, monsterPos.y - 0.60f);

                default: // 4º+ Melee: Forma uma segunda fila atrás dos primeiros
                    return new Vector2(monsterPos.x - (0.75f + (meleeIndex * 0.4f)), monsterPos.y);
            }
        }

        private int GetMeleeQueueIndex(Monster monster)
        {
            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            int index = 0;

            foreach (Hero h in heroes)
            {
                if (h == character || h == null || !h.IsAlive) continue;

                HeroBrain otherBrain = h.GetComponent<HeroBrain>();
                if (otherBrain != null && !otherBrain.IsRangedHero)
                {
                    Vector2 otherPos = GetEntityFeetPos(h);
                    Vector2 monsterPos = GetEntityFeetPos(monster);

                    float myDist = Vector2.Distance(MyFeetPos, monsterPos);
                    float otherDist = Vector2.Distance(otherPos, monsterPos);

                    if (otherDist < myDist - 0.05f)
                    {
                        index++;
                    }
                    else if (Mathf.Abs(myDist - otherDist) <= 0.05f)
                    {
                        if (h.gameObject.GetEntityId() < gameObject.GetEntityId())
                        {
                            index++;
                        }
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// TESTE 1: Histerese de alcance refinada
        /// </summary>
        private bool IsTargetInAttackRange(Monster monster, Vector2 targetAttackPos)
        {
            if (monster == null || !monster.IsAlive)
                return false;

            Vector2 monsterFeetPos = GetEntityFeetPos(monster);

            float tolerance = (_currentState == BrainState.InCombat) ? 0.7f : 0.25f;

            float diffX = Mathf.Abs(MyFeetPos.x - targetAttackPos.x);
            float diffY = Mathf.Abs(MyFeetPos.y - targetAttackPos.y);

            bool closeToSlot = diffX <= tolerance && diffY <= (Y_THRESHOLD + tolerance);

            float distToMonster = Vector2.Distance(MyFeetPos, monsterFeetPos);
            bool closeToMonster = distToMonster <= (character.Stats.attackRange + 0.3f);

            return closeToSlot || closeToMonster;
        }

        /// <summary>
        /// TESTE 2: Desbloqueio próximo à região de combate
        /// </summary>
        private bool IsBlockedByAlly()
        {
            if (_currentState != BrainState.MovingToTarget)
                return false;

            Monster targetMonster = FindTargetMonster();

            if (targetMonster != null)
            {
                float distToMonster = Vector2.Distance(MyFeetPos, GetEntityFeetPos(targetMonster));

                if (distToMonster < 1.2f)
                    return false;
            }

            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);

            foreach (Hero h in heroes)
            {
                if (h == character || h == null || !h.IsAlive)
                    continue;

                Vector2 otherFeetPos = GetEntityFeetPos(h);

                float deltaX = otherFeetPos.x - MyFeetPos.x;
                float deltaY = Mathf.Abs(otherFeetPos.y - MyFeetPos.y);

                if (deltaX > 0.1f && deltaX < 0.5f && deltaY < Y_THRESHOLD)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// PROVA DO CRIME: Rastreamento estrito de transições de estado
        /// </summary>
        public void SetState(BrainState newState)
        {
            if (_currentState != newState)
            {
                // Debug.Log($"🧠 [{name}] {_currentState} → {newState} | ID={gameObject.GetEntityId()}");
            }

            _currentState = newState;
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

        private void ExecuteMovement(Vector2 moveDirection, float lookDirectionX)
        {
            ApplyVisualFlip(lookDirectionX);

            if (moveDirection == Vector2.zero || IsBlockedByAlly())
            {
                if (_hero != null) _hero.SetState(0);
                character.Move(Vector2.zero);
                return;
            }

            if (_hero != null) _hero.SetState(2);
            character.Move(moveDirection);
        }

        private Vector2 CalculateTwoPhaseMovement(Vector2 currentPos, Vector2 targetPos)
        {
            float deltaY = targetPos.y - currentPos.y;
            float deltaX = targetPos.x - currentPos.x;

            if (Mathf.Abs(deltaY) > Y_THRESHOLD)
                return new Vector2(0f, Mathf.Sign(deltaY));

            if (Mathf.Abs(deltaX) > 0.05f)
                return new Vector2(Mathf.Sign(deltaX), 0f);

            return Vector2.zero;
        }

        private void ApplyVisualFlip(float directionX)
        {
            if (Mathf.Abs(directionX) < 0.01f) return;
            Vector3 scale = transform.localScale;
            scale.x = directionX < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        private Vector2 GetEntityFeetPos(Character entity)
        {
            return entity.FeetPoint != null ? (Vector2)entity.FeetPoint.position : (Vector2)entity.transform.position;
        }

        private Monster FindTargetMonster()
        {
            Character[] potentialTargets = FindObjectsByType<Monster>(FindObjectsInactive.Exclude);

            if (potentialTargets == null || potentialTargets.Length == 0) return null;

            if (_targetStrategy != null)
            {
                Character selectedCharacter = _targetStrategy.SelectTarget(this.character, potentialTargets);
                return selectedCharacter as Monster;
            }

            return FindClosestMonsterFallback(potentialTargets);
        }

        private Monster FindClosestMonsterFallback(Character[] targets)
        {
            Vector2 myPos = MyFeetPos;
            Monster closest = null;
            float minDist = float.MaxValue;

            foreach (Character t in targets)
            {
                if (t == null || !t.IsAlive) continue;
                float dist = Vector2.Distance(myPos, GetEntityFeetPos(t));
                if (dist < minDist) 
                { 
                    minDist = dist; 
                    closest = t as Monster; 
                }
            }

            return closest;
        }
    }
}