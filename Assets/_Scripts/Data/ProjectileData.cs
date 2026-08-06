using UnityEngine;

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Poison,
    Lightning,
    Holy,
    Dark
}

public enum SkillTargetType
{
    Enemy,
    Ally,
    Self,
    Ground,
    Area
}

[CreateAssetMenu(
    fileName = "NewProjectileData",
    menuName = "Dungeon/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    // ─────────────────────────────────────────────
    // VISUAL
    // ─────────────────────────────────────────────

    [Header("Visual")]

    public GameObject prefab;
    public Sprite sprite;

    public RuntimeAnimatorController animatorController;

    [Tooltip("Compensa sprites desenhados em outra direção.")]
    public float visualRotationOffset = 0f;

    public bool rotateSprite = true;

    public Color tint = Color.white;

    public Vector2 scale = Vector2.one;

    public GameObject trailVFX;

    public GameObject impactVFX;

    // ─────────────────────────────────────────────
    // AUDIO
    // ─────────────────────────────────────────────

    [Header("Audio")]

    public AudioClip castSFX;

    public AudioClip hitSFX;

    // ─────────────────────────────────────────────
    // COMBAT
    // ─────────────────────────────────────────────

    [Header("Combat")]

    public int damage = 10;

    public DamageType damageType = DamageType.Physical;

    [Range(0f, 1f)]
    public float critChance = 0f;

    public float critMultiplier = 2f;

    public float knockback = 0f;

    public float impactRadius = 0f;

    public SkillTargetType targetType = SkillTargetType.Enemy;

    // ─────────────────────────────────────────────
    // MOVEMENT
    // ─────────────────────────────────────────────

    [Header("Movement")]

    public float speed = 8f;

    public float lifetime = 5f;

    public ProjectileBehavior behavior = ProjectileBehavior.Straight;

    public bool destroyOnHit = true;

    public int maxBounce = 3;

    public float homingStrength = 3f;

    public int maxTargets = 1;

    // ─────────────────────────────────────────────
    // SPAWN
    // ─────────────────────────────────────────────

    [Header("Spawn")]

    public Vector2 spawnOffset = Vector2.zero;

    public float destroyDelay = 0f;

    public bool attachTrail = true;

    // ─────────────────────────────────────────────
    // FIRE MODE
    // ─────────────────────────────────────────────

    [Header("Fire Mode")]
    public bool  useVolley   = false;
    public int   volleyCount = 3;
    public float spreadAngle = 30f;
}