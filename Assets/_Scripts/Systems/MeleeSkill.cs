using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class MeleeSkill : MonoBehaviour
    {
        [SerializeField] private MeleeSkillData _meleeData;
        private Character _character;
        private float _timer;
        private int _bonusDamage;
        private float _bonusRange;
        private int _bonusCleave;
        private float _cooldownReduction;

        private void Awake() { _character = GetComponent<Character>(); }
        public void Initialize(MeleeSkillData data) { _meleeData = data; _timer = 0f; }

        private void Update()
        {
            // MonsterBrain owns the animation and impact timing for monsters.
            if (GetComponent<MonsterBrain>() != null || _meleeData == null ||
                _character == null || !_character.IsAlive || _character.Stats == null) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            Character closest = null;
            float distance = float.MaxValue;
            foreach (var candidate in FindObjectsByType<Character>(FindObjectsInactive.Exclude))
            {
                if (!IsEnemy(candidate)) continue;
                float d = Vector2.Distance(_character.CombatPoint.position, candidate.CombatPoint.position);
                if (d < distance) { closest = candidate; distance = d; }
            }
            if (TryAttack(closest)) _timer = Mathf.Max(0.2f, _meleeData.cooldown - _cooldownReduction);
        }

        public void ApplyUpgradeBonus(int extraDamage, float extraRange, int extraCleave, float cdr)
        {
            _bonusDamage += extraDamage; _bonusRange += extraRange;
            _bonusCleave += extraCleave; _cooldownReduction += cdr;
        }
        public float AttackInterval => Mathf.Max(0.2f, (_meleeData != null ? _meleeData.cooldown : 0.5f) - _cooldownReduction);

        public void ResetTalentUpgrades()
        {
            _bonusDamage = 0; _bonusRange = 0; _bonusCleave = 0; _cooldownReduction = 0;
        }

        public bool TryAttack(Character target)
        {
            if (_meleeData == null || _character == null || !_character.IsAlive ||
                _character.Stats == null || !IsEnemy(target)) return false;
            float range = Mathf.Max(0f, _meleeData.attackRange + _bonusRange);
            Vector2 origin = _character.CombatPoint.position;
            Vector2 direction = (Vector2)target.CombatPoint.position - origin;
            if (direction.sqrMagnitude > range * range) return false;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;
            PlayEffect(target, direction);
            int damage = Mathf.Max(0, _meleeData.baseDamage + _bonusDamage + _character.Stats.attackPower);
            var hit = new HashSet<Character>();
            hit.Add(target);
            Hit(target, damage, origin);
            int maxTargets = Mathf.Max(1, _meleeData.maxCleaveTargets + _bonusCleave);
            foreach (var col in Physics2D.OverlapCircleAll(origin, range))
            {
                if (hit.Count >= maxTargets) break;
                var other = col.GetComponentInParent<Character>();
                if (!IsEnemy(other) || hit.Contains(other)) continue;
                Vector2 delta = (Vector2)other.CombatPoint.position - origin;
                if (delta.sqrMagnitude > range * range || Vector2.Angle(direction, delta) > Mathf.Clamp(_meleeData.attackArcAngle, 0, 360) * 0.5f) continue;
                hit.Add(other);
                Hit(other, damage, origin);
            }
            return true;
        }

        private bool IsEnemy(Character target)
        {
            return target != null && target.IsAlive && target != _character &&
                ((_character is Monster && target is Hero) || (_character is Hero && target is Monster));
        }

        private void Hit(Character target, int damage, Vector2 origin)
        {
            var monster = _character as Monster;
            if (monster != null) monster.OnHitTarget(target, damage);
            else target.TakeDamage(damage);
            if (_meleeData.knockbackForce > 0f && target != null && target.IsAlive)
            {
                Vector2 direction = ((Vector2)target.CombatPoint.position - origin).normalized;
                target.transform.Translate(direction * _meleeData.knockbackForce * 0.1f, Space.World);
            }
            if (_meleeData.hitSFX != null) AudioSource.PlayClipAtPoint(_meleeData.hitSFX, origin);
        }

        private void PlayEffect(Character target, Vector2 direction)
        {
            if (_meleeData.attackVFXPrefab != null)
            {
                Vector3 position = _meleeData.placeVFXAtFeet ? target.FeetPoint.position : _character.MeleePoint.position;
                var effect = Instantiate(_meleeData.attackVFXPrefab, position, Quaternion.identity);
                var scale = effect.transform.localScale;
                scale.x *= direction.x < 0f ? -1f : 1f;
                effect.transform.localScale = scale;
                int layer = 0;
                int order = int.MinValue;
                IncludeCharacterSorting(_character, ref layer, ref order);
                IncludeCharacterSorting(target, ref layer, ref order);
                Debug.Log($"MeleeSkill.PlayEffect: SortingLayerID={layer}, SortingOrder={order}");
                foreach (var visual in effect.GetComponentsInChildren<SpriteRenderer>())
                {
                    visual.sortingLayerID = layer;
                    visual.sortingOrder = order == int.MinValue ? 1 : Mathf.Min(order + 1, 32767);
                }
            }
            if (_meleeData.swingSFX != null) AudioSource.PlayClipAtPoint(_meleeData.swingSFX, _character.MeleePoint.position);
        }

        // A character's outer SortingGroup takes precedence over its individual sprites.
        private static void IncludeCharacterSorting(Character character, ref int layer, ref int order)
        {
            foreach (var renderer in character.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!renderer.enabled) continue;
                int candidateLayer = renderer.sortingLayerID;
                int candidateOrder = renderer.sortingOrder;
                foreach (var group in renderer.GetComponentsInParent<UnityEngine.Rendering.SortingGroup>())
                {
                    if (!group.isActiveAndEnabled) continue;
                    candidateLayer = group.sortingLayerID;
                    candidateOrder = group.sortingOrder;
                    if (group.sortAtRoot) break;
                }
                int value = SortingLayer.GetLayerValueFromID(candidateLayer);
                int currentValue = SortingLayer.GetLayerValueFromID(layer);
                if (order == int.MinValue || value > currentValue ||
                    (value == currentValue && candidateOrder > order))
                {
                    layer = candidateLayer;
                    order = candidateOrder;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_character == null || _meleeData == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_character.CombatPoint.position, _meleeData.attackRange + _bonusRange);
        }
    }
}
