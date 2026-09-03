using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class StatusEffectManager : MonoBehaviour
    {
        private Character _character;
        private CharacterMovement _movement;
        private List<BehaviorType> _activeEffects = new List<BehaviorType>();

        private void Awake()
        {
            _character = GetComponent<Character>();
            _movement = GetComponent<CharacterMovement>();
        }

        public bool HasEffect(BehaviorType effect) => _activeEffects.Contains(effect);

        // ── 🧪 DEBUFFS DE DANO CONTÍNUO (DOT) ──

        public void ApplyPoison(float damagePerSecond, float duration)
        {
            StartCoroutine(PoisonRoutine(damagePerSecond, duration));
        }

        private IEnumerator PoisonRoutine(float damage, float duration)
        {
            _activeEffects.Add(BehaviorType.StatusPoison);
            float elapsed = 0f;

            while (elapsed < duration && _character.IsAlive)
            {
                yield return new WaitForSeconds(1f);
                _character.TakeDamage(Mathf.RoundToInt(damage));
                elapsed += 1f;
            }

            _activeEffects.Remove(BehaviorType.StatusPoison);
        }

        public void ApplyBurn(float damagePerSecond, float duration)
        {
            StartCoroutine(BurnRoutine(damagePerSecond, duration));
        }

        private IEnumerator BurnRoutine(float damage, float duration)
        {
            _activeEffects.Add(BehaviorType.StatusBurn);
            float elapsed = 0f;

            while (elapsed < duration && _character.IsAlive)
            {
                yield return new WaitForSeconds(0.5f);
                _character.TakeDamage(Mathf.RoundToInt(damage / 2f));
                elapsed += 0.5f;
            }

            _activeEffects.Remove(BehaviorType.StatusBurn);
        }

        // ── ❄️ DEBUFFS E BUFFS DE VELOCIDADE/CONTROLE ──

        public void ApplySlow(float slowPercent, float duration)
        {
            if (_movement != null) _movement.ApplySlow(slowPercent, duration);
        }

        public void ApplyFreeze(float duration)
        {
            if (_movement != null) _movement.ApplyFreeze(duration);
        }

        public void ApplyHaste(float boostPercent, float duration)
        {
            if (_movement != null) _movement.ApplyHaste(boostPercent, duration);
        }
    }
}