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

        // Propriedade para checar se o herói é Ranged ou Melee
        private bool IsRangedHero => character.Stats != null && character.Stats.attackRange > 1.5f;

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
                // Calcula a posição ideal de combate respeitando a fila e se é Melee/Ranged
                Vector2 targetAttackPos = GetCalculatedAttackPosition(closestMonster);

                if (IsTargetInAttackRange(closestMonster, targetAttackPos))
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
            Monster closestMonster = FindClosestMonster();

            if (closestMonster == null || !closestMonster.IsAlive)
            {
                _pauseTimer = _postCombatPauseDuration;
                SetState(BrainState.MovingToTarget);
                return;
            }

            Vector2 targetAttackPos = GetCalculatedAttackPosition(closestMonster);

            if (!IsTargetInAttackRange(closestMonster, targetAttackPos))
            {
                SetState(BrainState.MovingToTarget);
                return;
            }

            ExecuteMovement(Vector2.zero, GetEntityFeetPos(closestMonster).x - MyFeetPos.x);

            if (_attackTimer <= 0f)
            {
                if (_hero != null) _hero.SetState(0);
                
                // Triggers de animação dependentes do tipo
                if (IsRangedHero)
                    character.Animator?.SetTrigger("Shoot"); 
                else
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

        /// <summary>
        /// Calcula onde este herói específico deve se posicionar para atacar o monstro sem empilhar nos aliados.
        /// </summary>
        private Vector2 GetCalculatedAttackPosition(Monster monster)
        {
            Vector2 monsterPos = GetEntityFeetPos(monster);

            // RANGED: Para a uma distância segura na mesma faixa
            if (IsRangedHero)
            {
                float safeDistance = character.Stats.attackRange * 0.8f;
                return new Vector2(monsterPos.x - safeDistance, monsterPos.y);
            }

            // MELEE: Descobre quantos outros heróis Melee já estão atacando este mesmo monstro
            int meleeIndex = GetMeleeQueueIndex(monster);

            // Distribuição em arco (Frente, Cima, Baixo)
            switch (meleeIndex)
            {
                case 0: // 1º Melee: Ataca diretamente pela frente
                    return new Vector2(monsterPos.x - 0.75f, monsterPos.y);

                case 1: // 2º Melee: Ataca ligeiramente acima (diagonal superior)
                    return new Vector2(monsterPos.x - 0.65f, monsterPos.y + 0.35f);

                case 2: // 3º Melee: Ataca ligeiramente abaixo (diagonal inferior)
                    return new Vector2(monsterPos.x - 0.65f, monsterPos.y - 0.35f);

                default: // Demais: Aguardam um pouco atrás do 1º
                    return new Vector2(monsterPos.x - (0.75f + (meleeIndex * 0.5f)), monsterPos.y);
            }
        }

        /// <summary>
        /// Retorna a posição deste herói na fila de combate melee contra o monstro
        /// </summary>
        private int GetMeleeQueueIndex(Monster monster)
        {
            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            int index = 0;

            foreach (Hero h in heroes)
            {
                if (h == character || h == null || !h.IsAlive) continue;

                // Se o outro herói também é Melee e está mais perto do monstro que eu, ele ganha prioridade
                HeroBrain otherBrain = h.GetComponent<HeroBrain>();
                if (otherBrain != null && !otherBrain.IsRangedHero)
                {
                    Vector2 otherPos = GetEntityFeetPos(h);
                    Vector2 monsterPos = GetEntityFeetPos(monster);

                    if (Vector2.Distance(otherPos, monsterPos) < Vector2.Distance(MyFeetPos, monsterPos))
                    {
                        index++;
                    }
                }
            }
            return index;
        }

        private bool IsTargetInAttackRange(Monster monster, Vector2 targetAttackPos)
        {
            Vector2 monsterFeetPos = GetEntityFeetPos(monster);
            float distToMonster = Vector2.Distance(MyFeetPos, monsterFeetPos);

            // Ranged ataca por distância pura
            if (IsRangedHero)
            {
                return distToMonster <= character.Stats.attackRange;
            }

            // Melee checa se chegou ao ponto de cerco calculado
            float diffX = Mathf.Abs(MyFeetPos.x - targetAttackPos.x);
            float diffY = Mathf.Abs(MyFeetPos.y - targetAttackPos.y);

            return diffX <= 0.2f && diffY <= Y_THRESHOLD + 0.25f;
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

        public void SetState(BrainState newState) => _currentState = newState;

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
                
                // Se o aliado está na frente E no mesmo ponto $Y$, bloqueia a passagem
                if (otherFeetPos.x > MyFeetPos.x && Mathf.Abs(otherFeetPos.y - MyFeetPos.y) < Y_THRESHOLD && Vector2.Distance(MyFeetPos, otherFeetPos) < 0.5f)
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