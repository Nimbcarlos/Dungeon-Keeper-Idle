using UnityEngine;
using System;
using System.Collections.Generic;

namespace DungeonKeeper
{
    public enum MonsterQuality
    {
        Common,      // Cap Lv. 10
        Uncommon,    // Cap Lv. 20
        Rare,        // Cap Lv. 30
        Epic,        // Cap Lv. 40
        Legendary    // Cap Lv. 50
    }

    public class Monster : Character
    {
        // ── PROPRIEDADES DE DADOS ─────────────────────────
        public MonsterData Data { get; private set; }
        public MonsterProgression Progression { get; private set; } // 🎯 FIX: Declarada a propriedade Progression
        public Vector3 GuardPosition { get; private set; }

        [Header("Qualidade & Raridade")]
        public MonsterQuality quality = MonsterQuality.Common;

        // ── PROGRESSÃO DE NÍVEL (Ponteiros para o objeto Progression) ───────────────────────────
        public int CurrentLevel => Progression != null ? Progression.currentLevel : 1;
        public int CurrentXP => Progression != null ? Progression.currentXP : 0;

        public int MaxLevel => GetMaxLevelForQuality();
        public bool IsMaxLevel => CurrentLevel >= MaxLevel;
        public int XPToNextLevel => Data != null ? Data.GetXPRequired(CurrentLevel + 1) : GetDefaultXPForNextLevel();

        // ── EVENTOS E COMPONENTES ─────────────────────────
        public event Action<int> OnLevelUp;
        public event Action<int> OnXPGained;

        private MonsterSkillTree _skillTree;

        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private static readonly int IsHurt = Animator.StringToHash("isHurt");
        private static readonly int IsDead = Animator.StringToHash("isDead");

        public int CurrentLaneIndex { get; set; } = 0;

        [Header("Status de Combate Extensivos")]
        [SerializeField] private float _critChance = 0.05f;
        [SerializeField] private float _critDamageMultiplier = 1.5f;

        public List<BehaviorType> ActiveBehaviors { get; private set; } = new List<BehaviorType>();

        public void SetAffixData(MonsterAffixData affixData)
        {
            if (affixData == null) return;
            ActiveBehaviors = affixData.behaviors;
        }

        // ── INICIALIZAÇÃO ─────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _skillTree = GetComponent<MonsterSkillTree>();
            Debug.Assert(_skillTree != null, $"Monster '{name}' não possui um componente MonsterSkillTree. Certifique-se de adicioná-lo.");
        }

        public void InitializeMonster(MonsterData data, MonsterProgression progression = null)
        {
            Data = data;
            Progression = progression ?? new MonsterProgression();

            // Aplica os atributos base calculados para o nível individual do monstro
            base.Initialize(data.GetStatsForLevel(Progression.currentLevel));

            // Configura componentes de ataque baseados no MonsterData
            SetupAttackComponents(data);

            // Inicializa a árvore acoplando o Data (blueprint) e a Progression (instância)
            if (_skillTree != null)
            {
                _skillTree.InitializeTree(Data, Progression);
            }
        }

        public void Initialize(MonsterData monsterData, int level = -1, int xp = -1)
        {
            Data = monsterData;
            GuardPosition = transform.position;

            if (Progression == null) Progression = new MonsterProgression();

            if (level >= 1) Progression.currentLevel = level;
            if (xp >= 0) Progression.currentXP = xp;

            Progression.currentLevel = Mathf.Clamp(Progression.currentLevel, 1, MaxLevel);

            SetupAttackComponents(monsterData);

            if (monsterData != null)
            {
                base.Initialize(monsterData.GetStatsForLevel(CurrentLevel));
            }

            if (_skillTree != null)
            {
                _skillTree.InitializeTree(Data, Progression);
            }
        }

        private void SetupAttackComponents(MonsterData monsterData)
        {
            if (monsterData == null) return;

            if (monsterData.attackType == AttackType.Ranged)
            {
                ProjectileSkill rangedSkill = gameObject.GetComponent<ProjectileSkill>();
                if (rangedSkill == null) rangedSkill = gameObject.AddComponent<ProjectileSkill>();
                
                MeleeSkill melee = GetComponent<MeleeSkill>();
                if (melee != null) Destroy(melee);
            }
            else if (monsterData.attackType == AttackType.Melee)
            {
                MeleeSkill meleeSkill = gameObject.GetComponent<MeleeSkill>();
                if (meleeSkill == null) meleeSkill = gameObject.AddComponent<MeleeSkill>();
                
                meleeSkill.Initialize(monsterData.meleeData);

                ProjectileSkill ranged = GetComponent<ProjectileSkill>();
                if (ranged != null) Destroy(ranged);
            }
        }

        // ── XP E LEVEL UP ─────────────────────────────────

        public void GainXP(int amount)
        {
            if (IsMaxLevel || Progression == null) return;

            Progression.currentXP += amount;
            Debug.Log($"[{name}] Ganhou {amount} XP. Total atual: {CurrentXP}/{XPToNextLevel} para o próximo nível.");

            OnXPGained?.Invoke(CurrentXP);

            Color xpColor = new Color(0.7f, 0.3f, 1f);
            DamageTextManager.Instance?.SpawnDamageText(HeadPoint != null ? HeadPoint.position : transform.position, $"+{amount} XP", xpColor);

            while (!IsMaxLevel && CurrentXP >= XPToNextLevel)
            {
                Progression.currentXP -= XPToNextLevel;
                Progression.currentLevel++;

                if (Data != null)
                {
                    base.Initialize(Data.GetStatsForLevel(CurrentLevel));
                }

                OnLevelUp?.Invoke(CurrentLevel);
                _skillTree?.OnLevelUp(CurrentLevel);

                Color levelUpColor = GetQualityColor();
                DamageTextManager.Instance?.SpawnDamageText(HeadPoint != null ? HeadPoint.position : transform.position, "LEVEL UP!", levelUpColor, 7f);
            }
        }

        // ── REGRAS DE QUALIDADE E LIMITES ─────────────────

        public int GetMaxLevelForQuality()
        {
            switch (quality)
            {
                case MonsterQuality.Common:    return 10;
                case MonsterQuality.Uncommon:  return 20;
                case MonsterQuality.Rare:      return 30;
                case MonsterQuality.Epic:      return 40;
                case MonsterQuality.Legendary: return 50;
                default: return 10;
            }
        }

        public Color GetQualityColor()
        {
            switch (quality)
            {
                case MonsterQuality.Common:    return Color.white;
                case MonsterQuality.Uncommon:  return Color.green;
                case MonsterQuality.Rare:      return new Color(0f, 0.5f, 1f);
                case MonsterQuality.Epic:      return new Color(0.6f, 0f, 1f);
                case MonsterQuality.Legendary: return new Color(1f, 0.5f, 0f);
                default: return Color.white;
            }
        }

        private int GetDefaultXPForNextLevel()
        {
            float qualityXpMultiplier = GetQualityXPMultiplier();
            int baseXP = Mathf.RoundToInt(100 * Mathf.Pow(1.2f, CurrentLevel - 1));
            return Mathf.RoundToInt(baseXP * qualityXpMultiplier);
        }

        private float GetQualityXPMultiplier()
        {
            switch (quality)
            {
                case MonsterQuality.Common:    return 1.0f;
                case MonsterQuality.Uncommon:  return 1.2f;
                case MonsterQuality.Rare:      return 1.5f;
                case MonsterQuality.Epic:      return 1.8f;
                case MonsterQuality.Legendary: return 2.2f;
                default: return 1.0f;
            }
        }

        // ── SAVE SYSTEM INTEGRATION ───────────────────────

        public MonsterSaveState GetSaveData()
        {
            return new MonsterSaveState
            {
                monsterID = Data != null ? Data.id : "",
                currentLevel = CurrentLevel,
                currentXP = CurrentXP,
                quality = quality,
                position = GuardPosition,
                laneIndex = CurrentLaneIndex,
                unlockedSkillIDs = _skillTree != null ? _skillTree.GetUnlockedSkillIDs() : new List<string>()
            };
        }

        // ── ANIMAÇÕES ─────────────────────────────────────

        public void SetGuardPosition(Vector3 newPosition)
        {
            GuardPosition = newPosition;
            transform.position = newPosition;
        }

        protected override void OnAttack()
        {
            if (Animator == null) return;
            Animator.SetBool(IsMoving, false);
        }

        public void OnHitTarget(Character target, float baseDamage)
        {
            if (target == null || !target.IsAlive) return;

            target.TakeDamage(Mathf.RoundToInt(baseDamage));

            if (target.StatusEffects != null)
            {
                foreach (var behavior in ActiveBehaviors)
                {
                    switch (behavior)
                    {
                        case BehaviorType.StatusPoison:
                            target.StatusEffects.ApplyPoison(baseDamage * 0.15f, 4f);
                            break;

                        case BehaviorType.StatusBurn:
                            target.StatusEffects.ApplyBurn(baseDamage * 0.20f, 3f);
                            break;
                    }
                }
            }
        }

        public List<string> GetUnlockedSkillIDs()
        {
            return _skillTree != null ? _skillTree.GetUnlockedSkillIDs() : new List<string>();
        }

        public bool IsNodeUnlocked(string skillID)
        {
            return _skillTree != null && _skillTree.IsNodeUnlocked(skillID);
        }

        protected override void OnDieEffect()
        {
            if (Animator == null) return;
            Animator.SetBool(IsDead, true);
        }
    }
}