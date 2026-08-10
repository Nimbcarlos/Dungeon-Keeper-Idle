using UnityEngine;

namespace DungeonKeeper
{
    public class HeroBrain : CharacterBrain
    {
        private float _attackTimer;
        private Hero  _hero;
        private bool  _hasSacked;

        protected override void Awake()
        {
            base.Awake();
            _hero = GetComponent<Hero>();
        }

        protected override void Think()
        {
            // CRÍTICO: Se já saqueou ou o herói está morto, para o cérebro imediatamente!
            if (_hasSacked || _hero == null || !_hero.IsAlive) return;

            _attackTimer -= Time.deltaTime;

            Monster[] monsters = FindObjectsByType<Monster>(FindObjectsInactive.Exclude);
            Monster closest    = null;
            float minDist      = float.MaxValue;

            Vector2 myFeetPos = character.FeetPoint != null ? (Vector2)character.FeetPoint.position : (Vector2)transform.position;

            foreach (Monster m in monsters)
            {
                if (m == null || !m.IsAlive) continue;
                
                Vector2 monsterPos = m.FeetPoint != null ? (Vector2)m.FeetPoint.position : (Vector2)m.transform.position;
                float dist = Vector2.Distance(myFeetPos, monsterPos);

                if (dist < minDist) 
                { 
                    minDist = dist; 
                    closest = m; 
                }
            }

            // ── CASO 1: EXISTE MONSTRO VIVO NA SALA ──
            if (closest != null)
            {
                if (!InRangeOfGuard(closest))
                {
                    if (_hero != null) _hero.SetState(2); // Run (Correr)

                    if (!IsBlockedByAlly())
                    {
                        Vector2 targetPos = closest.GuardPosition;
                        Vector2 dir = (targetPos - myFeetPos).normalized;
                        
                        ApplyVisualFlip(dir.x); // Vira para a direção que está andando
                        character.Move(dir);
                    }
                }
                else
                {
                    if (_hero != null) _hero.SetState(0); // Stand (Parado)

                    // Garante que está olhando na direção do monstro
                    Vector2 monsterPos = closest.FeetPoint != null ? (Vector2)closest.FeetPoint.position : (Vector2)closest.transform.position;
                    ApplyVisualFlip(monsterPos.x - myFeetPos.x);

                    if (_attackTimer <= 0f)
                    {
                        character.Animator?.SetTrigger("Slash");
                        character.Attack(closest);
                        _attackTimer = 1f / character.Stats.attackSpeed;
                    }
                }
                return;
            }
            // ── CASO 2: SEM MONSTROS — VAI ATÉ O TESOURO ──
            Treasure treasure = FindAnyObjectByType<Treasure>();

            if (treasure == null)
            {
                if (_hero != null) _hero.SetState(2);
                ApplyVisualFlip(1f);
                if (!IsBlockedByAlly()) character.Move(Vector2.right);
                return;
            }

            float treasureX = treasure.transform.position.x;
            float distanceXToTreasure = treasureX - myFeetPos.x;

            if (distanceXToTreasure > 1.0f)
            {
                if (_hero != null) _hero.SetState(2); // Correndo para o tesouro
                ApplyVisualFlip(1f); 

                if (!IsBlockedByAlly()) character.Move(Vector2.right);
            }
            else
            {
                // CHEGOU NO TESOURO!
                _hasSacked = true;

                // 1. TRAVA A IA IMEDIATAMENTE (Desliga este script)
                this.enabled = false;

                Debug.Log("Herói chegou no tesouro — saqueando e comemorando!");
                
                // 2. Tira o ouro do baú
                if (_hero != null && _hero.Data != null)
                {
                    treasure.Sack(_hero.Data.goldReward);
                }

                // 3. Toca a animação de Vitória e faz desaparecer em 1.5s
                if (_hero != null)
                {
                    _hero.CelebrateVictoryAndDespawn(1.5f);
                }
            }
        }

        /// <summary>
        /// Inverte a escala X do personagem para ele olhar para onde está andando
        /// </summary>
        private void ApplyVisualFlip(float directionX)
        {
            if (Mathf.Abs(directionX) < 0.01f) return;

            Vector3 scale = transform.localScale;
            if (directionX < 0)
                scale.x = -Mathf.Abs(scale.x); // Olha para a Esquerda
            else
                scale.x = Mathf.Abs(scale.x);  // Olha para a Direita

            transform.localScale = scale;
        }

        bool IsBlockedByAlly()
        {
            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            Vector2 myFeetPos = character.FeetPoint != null ? (Vector2)character.FeetPoint.position : (Vector2)transform.position;

            foreach (Hero h in heroes)
            {
                if (h == character || h == null) continue;
                if (!h.IsAlive) continue;

                Vector2 otherFeetPos = h.FeetPoint != null ? (Vector2)h.FeetPoint.position : (Vector2)h.transform.position;

                if (otherFeetPos.x > myFeetPos.x &&
                    Vector2.Distance(myFeetPos, otherFeetPos) < 1.2f)
                    return true;
            }
            return false;
        }

        bool InRangeOfGuard(Monster target)
        {
            Vector2 myFeetPos = character.FeetPoint != null ? (Vector2)character.FeetPoint.position : (Vector2)transform.position;
            Vector2 targetPos = target.FeetPoint != null ? (Vector2)target.FeetPoint.position : (Vector2)target.GuardPosition;

            return Vector2.Distance(myFeetPos, targetPos) <= character.Stats.attackRange;
        }
    }
}