using UnityEngine;
using System;

namespace DungeonKeeper
{
    public class Monster : Character
    {
        // ── existentes ────────────────────────────────────
        public MonsterData Data         { get; private set; }
        public Vector3     GuardPosition { get; private set; }

        private static readonly int IsMoving    = Animator.StringToHash("isMoving");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private static readonly int IsHurt      = Animator.StringToHash("isHurt");
        private static readonly int IsDead      = Animator.StringToHash("isDead");

        // ── progressão ───────────────────────────────────
        public int CurrentLevel  { get; private set; } = 1;
        public int CurrentXP     { get; private set; } = 0;
        public bool IsMaxLevel   => Data != null && CurrentLevel >= Data.LevelCap;
        public int XPToNextLevel => Data != null ? Data.GetXPRequired(CurrentLevel + 1) : 100;

        public event Action<int> OnLevelUp;
        public event Action<int> OnXPGained;

        // ── inicialização ────────────────────────────────


        public void Initialize(MonsterData monsterData, int level = -1, int xp = -1)
        {
            Data          = monsterData;
            GuardPosition = transform.position;

            // usa parâmetros se passados explicitamente,
            // senão restaura do MonsterData (persistido no GainXP)
            int initialLevel = level >= 1 ? level : (monsterData != null ? monsterData.currentLevel : 1);
            int initialXP    = xp    >= 0 ? xp    : (monsterData != null ? monsterData.currentXP    : 0);

            CurrentLevel = Mathf.Clamp(initialLevel, 1, monsterData.LevelCap);
            CurrentXP    = initialXP;
            base.Initialize(monsterData.GetStatsForLevel(CurrentLevel));
        }

        // ── XP e Level Up ────────────────────────────────

        public void GainXP(int amount)
        {
            if (IsMaxLevel) return;
            CurrentXP += amount;
            
            // Sincroniza no Data imediatamente para garantir persistência no respawn
            if (Data != null)
            {
                Data.currentXP = CurrentXP;
                Data.currentLevel = CurrentLevel;
            }

            OnXPGained?.Invoke(CurrentXP);

            // Ganho de XP em roxo suave
            Color xpColor = new Color(0.7f, 0.3f, 1f);
            DamageTextManager.Instance?.SpawnDamageText(HeadPoint.position, $"+{amount} XP", xpColor);


            while (!IsMaxLevel && CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                CurrentLevel++;

                if (Data != null)
                {
                    Data.currentLevel = CurrentLevel;
                    Data.currentXP = CurrentXP;
                }

                base.Initialize(Data.GetStatsForLevel(CurrentLevel));
                OnLevelUp?.Invoke(CurrentLevel);
                GetComponent<MonsterSkillTree>()?.OnLevelUp();
                // Level Up em amarelo/dourado
                DamageTextManager.Instance?.SpawnDamageText(HeadPoint.position, "LEVEL UP!", Color.yellow, 7f);

            }
        }

        public MonsterSaveData GetSaveData() => new MonsterSaveData
        {
            id        = Data?.id,
            currentHP = Health.CurrentHP,
            level     = CurrentLevel,
            xp        = CurrentXP
        };

        // ── animações — preservadas ───────────────────────

        protected override void OnAttack()
        {
            if (Animator == null) return;
            Animator.SetBool(IsMoving, false);
        }

        protected override void OnHit() { }

        protected override void OnDieEffect()
        {
            if (Animator == null) return;
            Animator.SetBool(IsDead, true);
        }
    }
}