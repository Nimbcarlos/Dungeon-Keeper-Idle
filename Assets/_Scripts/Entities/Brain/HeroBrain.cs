using UnityEngine;

public class HeroBrain : CharacterBrain
{
    private float _attackTimer;
    private Hero  _hero; // referência direta ao Hero
    private bool _hasSacked;

    protected override void Awake()
    {
        base.Awake();
        _hero = GetComponent<Hero>();
    }

    protected override void Think()
    {
        _attackTimer -= Time.deltaTime;

        Monster[] monsters = FindObjectsByType<Monster>(FindObjectsInactive.Exclude);
        Monster closest    = null;
        float minDist      = float.MaxValue;

        foreach (Monster m in monsters)
        {
            if (!m.IsAlive) continue;
            float dist = Vector2.Distance(transform.position, m.transform.position);
            if (dist < minDist) { minDist = dist; closest = m; }
        }

        if (closest != null)
        {
            if (!InRangeOfGuard(closest))
            {
                character.Animator?.SetBool("isMoving",    true);
                character.Animator?.SetBool("isAttacking", false);

                if (!IsBlockedByAlly())
                {
                    Vector2 dir = ((Vector2)closest.GuardPosition
                        - (Vector2)transform.position).normalized;
                    character.Move(dir);
                }
            }
            else
            {
                character.Animator?.SetBool("isMoving", false);

                if (_attackTimer <= 0f)
                {
                    character.Animator?.SetBool("isAttacking", true);
                    character.Attack(closest);
                    _attackTimer = 1f / character.Stats.attackSpeed;
                }
            }
            return;
        }

        // sem monstro — vai até o tesouro
        Treasure treasure = FindAnyObjectByType<Treasure>();

        if (treasure == null)
        {
            character.Animator?.SetBool("isMoving",    true);
            character.Animator?.SetBool("isAttacking", false);
            if (!IsBlockedByAlly()) character.Move(Vector2.right);
            return;
        }

        float distToTreasure = Vector2.Distance(
            transform.position, treasure.transform.position);

        if (distToTreasure > character.Stats.attackRange)
        {
            character.Animator?.SetBool("isMoving",    true);
            character.Animator?.SetBool("isAttacking", false);
            if (!IsBlockedByAlly()) character.Move(Vector2.right);
        }
        else
        {
            if (!_hasSacked)
            {
                _hasSacked = true;
                Debug.Log("Herói chegou no tesouro — tentando sacar");
                
                if (_hero != null && _hero.Data != null)
                {
                    Debug.Log($"Sacking {_hero.Data.goldReward} gold");
                    treasure.Sack(_hero.Data.goldReward);
                }
                else
                {
                    Debug.Log($"_hero null: {_hero == null} | Data null: {_hero?.Data == null}");
                }

                _hero.Die();
            }
        }
    }

    bool IsBlockedByAlly()
    {
        Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
        foreach (Hero h in heroes)
        {
            if (h == character) continue;
            if (!h.IsAlive) continue;

            if (h.transform.position.x > transform.position.x &&
                Vector2.Distance(transform.position, h.transform.position) < 1.5f)
                return true;
        }
        return false;
    }

    bool InRangeOfGuard(Monster target)
    {
        return Vector2.Distance(
            transform.position,
            target.GuardPosition) <= character.Stats.attackRange;
    }
}