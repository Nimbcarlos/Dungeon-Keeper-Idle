using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private MonsterData _monsterData;
    [SerializeField] private Transform   _monsterSlot;
    [SerializeField] private Transform   _heroSpawn;
    [SerializeField] private Treasure    _treasure;

    [Header("Roster")]
    [SerializeField] private HeroRoster  _heroRoster;
    [SerializeField] private PartyConfig _partyConfig;

    [Header("Configuração")]
    [SerializeField] private float _delayBetweenParties  = 2f;
    [SerializeField] private float _delayBetweenMembers  = 1.5f;

    [Header("Respawn do Monstro")]
    [SerializeField] private float _monsterRespawnDelay = 3f;

    private Monster          _currentMonster;
    private List<Hero>       _currentParty = new List<Hero>();
    private bool             _partyActive  = false;

    void Start()
    {
        if (_monsterData != null && _monsterSlot != null)
            SpawnMonster();

        // escuta o saque do tesouro
        if (_treasure != null)
            _treasure.OnTreasureSacked += OnPartySacked;

        StartCoroutine(PartyLoop());
    }

    // ── loop de parties ──────────────────────────────

    IEnumerator PartyLoop()
    {
        while (true)
        {
            // aguarda a party anterior terminar
            yield return new WaitUntil(() => !_partyActive);

            yield return new WaitForSeconds(_delayBetweenParties);

            // Verificação de segurança para o Singleton do GameManager
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                continue;

            yield return StartCoroutine(SpawnParty());
        }
    }

    IEnumerator SpawnParty()
    {
        // Verificações críticas de referências
        if (_partyConfig == null)
        {
            Debug.LogError("SpawnManager: _partyConfig não está atribuído no Inspector!");
            yield break;
        }

        if (_heroRoster == null)
        {
            Debug.LogError("SpawnManager: _heroRoster não está atribuído no Inspector!");
            yield break;
        }

        _partyActive = true;
        _currentParty.Clear();

        // tamanho da party baseado no Gold atual
        int currentGold = (ResourceManager.Instance != null) ? ResourceManager.Instance.Gold : 0;
        int size        = _partyConfig.GetPartySize(currentGold);

        for (int i = 0; i < size; i++)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                yield break;

            // CORREÇÃO: O HeroRoster exige (int gold, int difficulty). 
            // Como a variável currentDifficulty não existe no contexto atual, usamos 1 como padrão.
            int defaultDifficulty = 1; 
            HeroData heroData = _heroRoster.GetRandom(currentGold, defaultDifficulty);

            if (heroData != null && heroData.prefab != null)
            {
                SpawnHero(heroData);
            }
            else
            {
                Debug.LogWarning("SpawnManager: Falha ao obter HeroData ou Prefab nulo para o Gold atual: " + currentGold);
            }

            if (i < size - 1)
                yield return new WaitForSeconds(_delayBetweenMembers);
        }
    }

    void SpawnHero(HeroData data)
    {
        if (_heroSpawn == null) return;

        GameObject obj = Instantiate(
            data.prefab,
            _heroSpawn.position,
            Quaternion.identity);

        Hero hero = obj.GetComponent<Hero>();
        if (hero != null)
        {
            hero.Initialize(data);
            if (hero.Health != null)
            {
                hero.Health.OnDeath += () => OnHeroDied(hero);
            }
            _currentParty.Add(hero);
        }
    }

    // ── eventos de fim de party ──────────────────────

    void OnHeroDied(Hero hero)
    {
        if (hero == null || hero.HasSacked) return;
        _currentParty.Remove(hero);
        CheckPartyFinished();
    }

    void OnPartySacked()
    {
        foreach (Hero h in _currentParty)
        {
            if (h != null)
                Destroy(h.gameObject);
        }
        _currentParty.Clear();
        _partyActive = false;
    }
 
    void CheckPartyFinished()
    {
        // party terminou quando todos morreram
        if (_currentParty.Count == 0)
            _partyActive = false;
    }

    // ── monstros ─────────────────────────────────────

    void SpawnMonster()
    {
        if (_monsterData == null || _monsterData.prefab == null || _monsterSlot == null) return;

        GameObject obj = Instantiate(
            _monsterData.prefab,
            _monsterSlot.position,
            Quaternion.identity);

        _currentMonster = obj.GetComponent<Monster>();
        if (_currentMonster != null)
        {
            _currentMonster.Initialize(_monsterData);
            if (_currentMonster.Health != null)
            {
                _currentMonster.Health.OnDeath += OnMonsterDied;
            }

            AssignBrain(obj, _monsterData.defaultBehavior);
        }
    }

    void OnMonsterDied()
    {
        StartCoroutine(RespawnMonsterAfterDelay());
    }

    IEnumerator RespawnMonsterAfterDelay()
    {
        yield return new WaitForSeconds(_monsterRespawnDelay);

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            yield break;

        SpawnMonster();
    }

    void AssignBrain(GameObject obj, MonsterBehavior behavior)
    {
        switch (behavior)
        {
            case MonsterBehavior.Defensive:
                if (obj.GetComponent<MonsterBrain>() == null)
                    obj.AddComponent<MonsterBrain>(); 
                break;
        }
    }
}
