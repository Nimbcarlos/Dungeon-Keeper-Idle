using UnityEngine;
using DungeonKeeper;

public class ProjectileSkill : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private ProjectileData _projectileData;

    [Header("Skill")]
    [SerializeField] private float _cooldown = 3f;

    private Character _character;
    private float     _timer;

    // adiciona esses campos privados
    private bool  _piercing;
    private bool  _bounceEnabled;
    private int   _volleyBonus;
    private float _spreadBonus;
    private int   _damageBonus;
    private float _speedBonus;
    private float _cooldownReduction;
    private int   _bounceBonus;

    void Awake()
    {
        _character = GetComponent<Character>();
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = _cooldown;
            Fire();
        }
    }

    public void ApplyUpgrade(SkillUpgrade upgrade)
    {
        if (upgrade.enablePiercing)    _piercing          = true;
        if (upgrade.enableBounce)      _bounceEnabled     = true;
        if (upgrade.enableVolley)      _projectileData.useVolley = true;
        if (upgrade.enableHoming)      _projectileData.behavior  = ProjectileBehavior.Homing;

        _volleyBonus       += upgrade.volleyCountBonus;
        _spreadBonus       += upgrade.spreadAngleBonus;
        _damageBonus       += upgrade.damageBonus;
        _speedBonus        += upgrade.speedBonus;
        _cooldownReduction += upgrade.cooldownReduction;
        _bounceBonus       += upgrade.maxBounceBonus;
    }

    void Fire()
    {
        ITargetable target = FindTarget();
        if (target == null) return;

        Vector2 baseDir = ((Vector2)target.Transform.position
            - (Vector2)_character.CombatPoint.position).normalized;

        if (_projectileData.useVolley)
            FireVolley(baseDir, target);
        else
            FireProjectile(baseDir, target);
    }

    void FireVolley(Vector2 baseDir, ITargetable target)
    {
        int   count  = _projectileData.volleyCount;
        float spread = _projectileData.spreadAngle;

        if (count <= 1)
        {
            FireProjectile(baseDir, target);
            return;
        }

        float startAngle = -(spread * 0.5f);
        float step       = spread / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float   angle = startAngle + step * i;
            Vector2 dir   = RotateVector(baseDir, angle);
            FireProjectile(dir, target);
        }
    }


    // no FireProjectile, aplica os modificadores
    void FireProjectile(Vector2 direction, ITargetable target)
    {
        if (_projectileData?.prefab == null) return;

        GameObject obj = Instantiate(
            _projectileData.prefab,
            _character.CombatPoint.position,
            Quaternion.identity);

        Projectile projectile = obj.GetComponent<Projectile>();
        if (projectile == null) return;

        // behavior final com upgrades
        ProjectileBehavior behavior = _projectileData.behavior;
        if (_piercing)     behavior = ProjectileBehavior.Piercing;
        if (_bounceEnabled) behavior = ProjectileBehavior.Bounce;

        // cria uma cópia dos dados com modificadores aplicados
        // sem modificar o ScriptableObject original
        projectile.Initialize(
            direction,
            gameObject,
            _projectileData,
            behavior,
            _damageBonus,
            _speedBonus,
            _bounceBonus,
            _projectileData.behavior == ProjectileBehavior.Homing ? target : null);
    }

    ITargetable FindTarget()
    {
        bool isMonster = GetComponent<Monster>() != null;

        if (isMonster)
        {
            Hero[] heroes = FindObjectsByType<Hero>(FindObjectsInactive.Exclude);
            Hero closest  = null;
            float minDist = float.MaxValue;

            foreach (Hero h in heroes)
            {
                if (!h.IsAlive) continue;
                float dist = Vector2.Distance(
                    _character.CombatPoint.position, h.transform.position);
                if (dist < minDist) { minDist = dist; closest = h; }
            }
            return closest;
        }
        else
        {
            Monster[] monsters = FindObjectsByType<Monster>(FindObjectsInactive.Exclude);
            Monster closest    = null;
            float minDist      = float.MaxValue;

            foreach (Monster m in monsters)
            {
                if (!m.IsAlive) continue;
                float dist = Vector2.Distance(
                    _character.CombatPoint.position, m.transform.position);
                if (dist < minDist) { minDist = dist; closest = m; }
            }
            return closest;
        }
    }

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}