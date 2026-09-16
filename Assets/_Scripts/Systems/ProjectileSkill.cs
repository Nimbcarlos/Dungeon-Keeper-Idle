using UnityEngine;
using DungeonKeeper;

public class ProjectileSkill : MonoBehaviour
{
    [SerializeField] private ProjectileData _projectileData;
    [SerializeField] private float _cooldown = 3f;
    private Character _character;
    private float _timer;
    private bool _piercing, _bounceEnabled, _volleyEnabled, _homingEnabled;
    private int _volleyBonus, _damageBonus, _bounceBonus;
    private float _spreadBonus, _speedBonus, _cooldownReduction;

    public float AttackInterval => Mathf.Max(0.2f, _cooldown - _cooldownReduction);

    private void Awake() { _character = GetComponent<Character>(); }

    public void Initialize(ProjectileData data, float cooldown)
    {
        _projectileData = data;
        _cooldown = Mathf.Max(0.2f, cooldown);
        _timer = 0f;
    }

    public void ResetTalentUpgrades()
    {
        _piercing = _bounceEnabled = _volleyEnabled = _homingEnabled = false;
        _volleyBonus = _damageBonus = _bounceBonus = 0;
        _spreadBonus = _speedBonus = _cooldownReduction = 0f;
    }

    private void Update()
    {
        // A monster with a brain fires at its animation impact, not from a second timer.
        if (GetComponent<MonsterBrain>() != null || _projectileData == null ||
            _character == null || !_character.IsAlive || _character.Stats == null) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f && TryFire(FindTarget())) _timer = AttackInterval;
    }

    public void ApplyUpgrade(SkillUpgrade upgrade)
    {
        if (upgrade == null) return;
        _piercing |= upgrade.enablePiercing;
        _bounceEnabled |= upgrade.enableBounce;
        _volleyEnabled |= upgrade.enableVolley;
        _homingEnabled |= upgrade.enableHoming;
        _volleyBonus += upgrade.volleyCountBonus;
        _spreadBonus += upgrade.spreadAngleBonus;
        _damageBonus += upgrade.damageBonus;
        _speedBonus += upgrade.speedBonus;
        _cooldownReduction += upgrade.cooldownReduction;
        _bounceBonus += upgrade.maxBounceBonus;
    }

    public bool TryFire(Character target)
    {
        if (_projectileData == null || _projectileData.prefab == null ||
            _character == null || !_character.IsAlive || _character.Stats == null || !IsEnemy(target)) return false;
        Vector2 delta = target.CombatPoint.position - _character.CombatPoint.position;
        if (delta.sqrMagnitude > _character.Stats.attackRange * _character.Stats.attackRange) return false;
        Vector2 offset = _projectileData.spawnOffset;
        offset.x *= delta.x < 0f ? -1f : 1f;
        Vector3 spawnPosition = _character.RangedPoint.position + (Vector3)offset;
        delta = target.CombatPoint.position - spawnPosition;
        Vector2 direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.right;
        int baseCount = _projectileData.useVolley || _volleyEnabled ? _projectileData.volleyCount : 1;
        int count = Mathf.Max(1, baseCount + _volleyBonus);
        float spread = Mathf.Clamp(_projectileData.spreadAngle + _spreadBonus, 0f, 360f);
        for (int i = 0; i < count; i++)
        {
            float angle = count == 1 ? 0f : -spread * 0.5f + spread * i / (count - 1);
            Vector2 shotDirection = Quaternion.Euler(0, 0, angle) * direction;
            FireProjectile(shotDirection, target, spawnPosition);
        }
        if (_projectileData.castSFX != null)
            AudioSource.PlayClipAtPoint(_projectileData.castSFX, spawnPosition);
        return true;
    }

    private void FireProjectile(Vector2 direction, Character target, Vector3 spawnPosition)
    {
        var obj = Instantiate(_projectileData.prefab, spawnPosition, Quaternion.identity);
        var projectile = obj.GetComponent<Projectile>();
        if (projectile == null) { Destroy(obj); return; }
        var source = _character.SpriteRenderer;
        if (source != null)
            foreach (var renderer in obj.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.sortingLayerID = source.sortingLayerID;
                renderer.sortingOrder = source.sortingOrder + 1;
            }
        ProjectileBehavior behavior = _projectileData.behavior;
        if (_homingEnabled) behavior = ProjectileBehavior.Homing;
        if (_piercing) behavior = ProjectileBehavior.Piercing;
        if (_bounceEnabled) behavior = ProjectileBehavior.Bounce;
        if (behavior == ProjectileBehavior.ArcShot)
        {
            projectile.InitializeArc(target.CombatPoint.position, 1f, gameObject, _projectileData);
            return;
        }
        projectile.Initialize(direction, gameObject, _projectileData, behavior,
            _damageBonus + _character.Stats.attackPower, _speedBonus, _bounceBonus,
            behavior == ProjectileBehavior.Homing ? target : null);
    }

    private bool IsEnemy(Character target)
    {
        return target != null && target.IsAlive &&
            ((_character is Monster && target is Hero) || (_character is Hero && target is Monster));
    }

    private Character FindTarget()
    {
        Character closest = null;
        float distance = float.MaxValue;
        foreach (var target in FindObjectsByType<Character>(FindObjectsInactive.Exclude))
        {
            if (!IsEnemy(target)) continue;
            float d = Vector2.Distance(_character.CombatPoint.position, target.CombatPoint.position);
            if (d < distance) { closest = target; distance = d; }
        }
        return closest;
    }
}
