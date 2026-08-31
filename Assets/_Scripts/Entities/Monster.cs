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
        public MonsterData Data          { get; private set; }
        public Vector3     GuardPosition { get; private set; }

        [Header("Qualidade & Raridade")]
        public MonsterQuality quality = MonsterQuality.Common;

        // ── PROGRESSÃO DE NÍVEL ───────────────────────────
        public int CurrentLevel  { get; private set; } = 1;
        public int CurrentXP     { get; private set; } = 0;

        public int MaxLevel      => GetMaxLevelForQuality();
        public bool IsMaxLevel   => CurrentLevel >= MaxLevel;
        public int XPToNextLevel => Data != null ? Data.GetXPRequired(CurrentLevel + 1) : GetDefaultXPForNextLevel();

        // ── EVENTOS E COMPONENTES ─────────────────────────
        public event Action<int> OnLevelUp;
        public event Action<int> OnXPGained;

        private MonsterSkillTree _skillTree;
        private List<string> _unlockedSkillIDs = new List<string>();

        private static readonly int IsMoving    = Animator.StringToHash("isMoving");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private static readonly int IsHurt      = Animator.StringToHash("isHurt");
        private static readonly int IsDead      = Animator.StringToHash("isDead");

        public int CurrentLaneIndex { get; set; } = 0;

        [Header("Status de Combate Extensivos")]
        [SerializeField] private float _critChance = 0.05f; // 5% de chance base
        [SerializeField] private float _critDamageMultiplier = 1.5f; // 150% de dano

        // Lista de comportamentos que o monstro possui (gerados via MonsterGenerator)
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

        public void Initialize(MonsterData monsterData, int level = -1, int xp = -1)
        {
            Data          = monsterData;
            GuardPosition = transform.position;

            int initialLevel = level >= 1 ? level : (monsterData != null ? monsterData.currentLevel : 1);
            int initialXP    = xp    >= 0 ? xp    : (monsterData != null ? monsterData.currentXP    : 0);

            CurrentLevel = Mathf.Clamp(initialLevel, 1, MaxLevel);
            CurrentXP    = initialXP;

            if (monsterData != null)
            {
                base.Initialize(monsterData.GetStatsForLevel(CurrentLevel));
            }
        }

        // ── XP E LEVEL UP ─────────────────────────────────

        public void GainXP(int amount)
        {
            if (IsMaxLevel) return;

            CurrentXP += amount;
            Debug.Log($"[{name}] Ganhou {amount} XP. Total atual: {CurrentXP}/{XPToNextLevel} para o próximo nível.");

            // Sincroniza no Data para persistência de respawn
            if (Data != null)
            {
                Data.currentXP = CurrentXP;
                Data.currentLevel = CurrentLevel;
            }

            OnXPGained?.Invoke(CurrentXP);

            // Floating Text de XP em roxo
            Color xpColor = new Color(0.7f, 0.3f, 1f);
            DamageTextManager.Instance?.SpawnDamageText(HeadPoint != null ? HeadPoint.position : transform.position, $"+{amount} XP", xpColor);

            // Loop de Level Up
            while (!IsMaxLevel && CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                CurrentLevel++;

                if (Data != null)
                {
                    Data.currentLevel = CurrentLevel;
                    Data.currentXP    = CurrentXP;
                    base.Initialize(Data.GetStatsForLevel(CurrentLevel));
                }

                // Notifica inscritos e a árvore de habilidades
                OnLevelUp?.Invoke(CurrentLevel);
                _skillTree?.OnLevelUp(CurrentLevel);

                // Floating Text de Level Up
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
                case MonsterQuality.Rare:      return new Color(0f, 0.5f, 1f);   // Azul
                case MonsterQuality.Epic:      return new Color(0.6f, 0f, 1f);   // Roxo
                case MonsterQuality.Legendary: return new Color(1f, 0.5f, 0f);   // Laranja
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
            MonsterSkillTree skillTree = GetComponent<MonsterSkillTree>();

            return new MonsterSaveState
            {
                monsterID        = Data != null ? Data.id : "",
                currentLevel     = CurrentLevel,
                currentXP        = CurrentXP,
                quality          = quality,
                position         = GuardPosition,
                laneIndex        = CurrentLaneIndex, // Incluído
                unlockedSkillIDs = skillTree != null ? skillTree.GetUnlockedSkillIDs() : new List<string>()
            };
}
        // ── ANIMAÇÕES ─────────────────────────────────────

        public void SetGuardPosition(Vector3 newPosition)
        {
            GuardPosition = newPosition;
            transform.position = newPosition; // Garante que ele já vá para o ponto certo
        }

        protected override void OnAttack()
        {
            if (Animator == null) return;
            Animator.SetBool(IsMoving, false);
        }

        public void OnHitTarget(Character target, float baseDamage)
        {
            if (target == null || !target.IsAlive) return;

            // Aplica o dano direto no Character/Health
            target.TakeDamage(Mathf.RoundToInt(baseDamage));

            // Se o alvo possui o gerenciador de status, repassa os efeitos
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

        // Retorna a lista de IDs para o Save System no Monster.cs
        public List<string> GetUnlockedSkillIDs()
        {
            return _unlockedSkillIDs ?? new List<string>();
        }

        // Carrega os IDs ao restaurar o Save
        public void SetUnlockedSkillIDs(List<string> unlockedIDs)
        {
            _unlockedSkillIDs = unlockedIDs ?? new List<string>();
        }

        // Verifica se um nó específico já foi comprado
        public bool IsNodeUnlocked(string skillID)
        {
            return _unlockedSkillIDs != null && _unlockedSkillIDs.Contains(skillID);
        }

        protected override void OnDieEffect()
        {
            if (Animator == null) return;
            Animator.SetBool(IsDead, true);
        }
    }
}
