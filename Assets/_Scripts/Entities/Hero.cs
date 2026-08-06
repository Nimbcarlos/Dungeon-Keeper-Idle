using UnityEngine;

public class Hero : Character
{
    public HeroData Data { get; private set; }

    private static readonly int IsMoving    = Animator.StringToHash("isMoving");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int IsHurt      = Animator.StringToHash("isHurt");
    private static readonly int IsDead      = Animator.StringToHash("isDead");

    public bool HasSacked { get; set; }

    public void Initialize(HeroData heroData)
    {
        Data = heroData;
        base.Initialize(heroData.stats);
    }

    protected override void OnAttack()
    {
        if (Animator == null) return;
        Animator.SetBool(IsMoving,    false);
        Animator.SetBool(IsAttacking, true);
    }

    protected override void OnHit()
    {
        // if (Animator == null) return;
        // Animator.SetBool(IsHurt, true);
    }

    protected override void OnDieEffect()
    {
        if (Data == null) return;

        // ouro vai pro baú
        Treasure treasure = FindAnyObjectByType<Treasure>();
        if (treasure != null) treasure.AddGold(Data.goldReward);

        // essência vai pro jogador
        ResourceManager.Instance.AddEssence(Data.essenceReward);

        Debug.Log($"Herói derrotado: +{Data.goldReward} Gold no baú, +{Data.essenceReward} Essência");
        if (Animator == null) return;
        Animator.SetBool(IsDead, true);

    }
    public new void Die()
    {
        // OnDieEffect();
        Destroy(gameObject, 0.1f);
    }
}
