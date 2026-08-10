using UnityEngine;
using System;

namespace DungeonKeeper
{
    public class Health : MonoBehaviour
    {
        public int CurrentHP { get; private set; }
        public int MaxHP     { get; private set; }
        public bool IsDead   => CurrentHP <= 0;
        public float Percent => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

        public event Action      OnDeath;
        public event Action<int> OnDamageTaken;
        public event Action<int> OnHealed;

        public void Initialize(Stats stats)
        {
            MaxHP     = stats.maxHP;
            CurrentHP = stats.maxHP;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            OnDamageTaken?.Invoke(amount);
            if (IsDead) OnDeath?.Invoke();
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            OnHealed?.Invoke(amount);
        }
    }
}