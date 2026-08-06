using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonsterSkillTree : MonoBehaviour
{
    [Header("Árvore de Skills")]
    [SerializeField] private SkillUpgrade _rootUpgrade; // primeiro nó disponível no Lv1

    public List<SkillUpgrade> AppliedUpgrades { get; private set; } = new();
    public SkillUpgrade[]     PendingOptions  { get; private set; }
    public bool               HasPendingChoice => PendingOptions != null && PendingOptions.Length > 0;

    private ProjectileSkill _projectileSkill;

    void Awake()
    {
        _projectileSkill = GetComponent<ProjectileSkill>();
    }

    // chamado quando monstro sobe de nível
    public void OnLevelUp()
    {
        if (AppliedUpgrades.Count == 0 && _rootUpgrade != null)
        {
            // primeiro nível — oferece as opções do root
            PendingOptions = _rootUpgrade.nextOptions;
        }
        else if (AppliedUpgrades.Count > 0)
        {
            // próximo nível — opções do último upgrade escolhido
            SkillUpgrade last = AppliedUpgrades.Last();
            PendingOptions = last.nextOptions;
        }

        if (HasPendingChoice)
            // SkillChoiceUI.Instance?.Show(this);
            Debug.Log($"Escolha de skill disponível para {gameObject.name}: {string.Join(" | ", PendingOptions.Select(o => o.displayName))}");
    }

    // chamado quando jogador escolhe um upgrade
    public void ApplyUpgrade(SkillUpgrade upgrade)
    {
        AppliedUpgrades.Add(upgrade);
        PendingOptions = null;

        if (_projectileSkill != null)
            _projectileSkill.ApplyUpgrade(upgrade);

        Debug.Log($"Upgrade aplicado: {upgrade.displayName}");
    }

    // restaura upgrades salvos ao respawnar
    public void RestoreUpgrades(List<string> upgradeIds, List<SkillUpgrade> allUpgrades)
    {
        foreach (string id in upgradeIds)
        {
            SkillUpgrade upgrade = allUpgrades.Find(u => u.id == id);
            if (upgrade != null) ApplyUpgrade(upgrade);
        }
    }
}