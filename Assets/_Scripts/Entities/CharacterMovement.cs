using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class CharacterMovement : MonoBehaviour
    {
        private Character _character;

        public bool IsStunned { get; private set; }
        public bool IsFrozen { get; private set; }
        public bool CanMove => !IsStunned && !IsFrozen;

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        public void Move(Vector2 direction)
        {
            if (!CanMove || _character == null) return;
            transform.Translate(direction * _character.Stats.moveSpeed * Time.deltaTime, Space.World);
        }

        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            StartCoroutine(KnockbackRoutine(direction.normalized, force, duration));
        }

        private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && _character.IsAlive)
            {
                transform.Translate(direction * force * Time.deltaTime, Space.World);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        public void ApplySlow(float slowPercent, float duration) => StartCoroutine(SlowRoutine(slowPercent, duration));

        private IEnumerator SlowRoutine(float slowPercent, float duration)
        {
            float originalSpeed = _character.Stats.moveSpeed;
            _character.Stats.moveSpeed *= (1f - Mathf.Clamp01(slowPercent));

            yield return new WaitForSeconds(duration);

            _character.Stats.moveSpeed = originalSpeed;
        }

        public void ApplyHaste(float boostPercent, float duration) => StartCoroutine(HasteRoutine(boostPercent, duration));

        private IEnumerator HasteRoutine(float boostPercent, float duration)
        {
            float originalSpeed = _character.Stats.moveSpeed;
            _character.Stats.moveSpeed *= (1f + boostPercent);

            yield return new WaitForSeconds(duration);

            _character.Stats.moveSpeed = originalSpeed;
        }

        public void ApplyFreeze(float duration) => StartCoroutine(FreezeRoutine(duration));

        private IEnumerator FreezeRoutine(float duration)
        {
            IsFrozen = true;
            yield return new WaitForSeconds(duration);
            IsFrozen = false;
        }
    }
}