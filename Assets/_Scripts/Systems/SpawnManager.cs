using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKeeper
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private Transform   _heroSpawn;
        [SerializeField] private Treasure    _treasure;

        [Header("Roster")]
        [SerializeField] private HeroRoster  _heroRoster;
        [SerializeField] private PartyConfig _partyConfig;

        [Header("Configuração")]
        [SerializeField] private float _delayBetweenParties  = 2f;
        [SerializeField] private float _delayBetweenMembers  = 1.5f;

        private List<Hero> _currentParty = new List<Hero>();
        private bool       _partyActive  = false;

        void Start()
        {
            // Escuta o saque do tesouro
            if (_treasure != null)
                _treasure.OnTreasureSacked += OnPartySacked;

            StartCoroutine(PartyLoop());
        }

        // ── loop de parties ──────────────────────────────

        IEnumerator PartyLoop()
        {
            while (true)
            {
                yield return new WaitUntil(() => !_partyActive);
                yield return new WaitForSeconds(_delayBetweenParties);

                if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                    continue;

                yield return StartCoroutine(SpawnParty());
            }
        }

        IEnumerator SpawnParty()
        {
            if (_partyConfig == null || _heroRoster == null)
            {
                Debug.LogError("SpawnManager: Configurações ausentes no Inspector!");
                yield break;
            }

            _partyActive = true;
            _currentParty.Clear();

            int currentGold = (ResourceManager.Instance != null) ? ResourceManager.Instance.Gold : 0;
            int size        = _partyConfig.GetPartySize(currentGold);

            for (int i = 0; i < size; i++)
            {
                if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                    yield break;

                int defaultDifficulty = 1; 
                HeroData heroData = _heroRoster.GetRandom(currentGold, defaultDifficulty);

                if (heroData != null && heroData.prefab != null)
                {
                    SpawnHero(heroData);
                }

                if (i < size - 1)
                    yield return new WaitForSeconds(_delayBetweenMembers);
            }
        }

        void SpawnHero(HeroData data)
        {
            if (_heroSpawn == null) return;

            GameObject obj = Instantiate(data.prefab, _heroSpawn.position, Quaternion.identity);

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
                {
                    // 1. Se foi ELE que encostou no baú, não destruímos aqui!
                    // Deixamos a Coroutine CelebrateVictoryAndDespawn() dele rodar até o fim.
                    if (h.HasSacked) 
                        continue;

                    // 2. Se for outro herói da party (ex: um Tank/Healer que ficou mais atrás),
                    // ele também venceu a run! Fazemos ele comemorar e sumir junto.
                    h.CelebrateVictoryAndDespawn(1.5f);
                }
            }

            _currentParty.Clear();
            _partyActive = false; // Permite o SpawnManager iniciar a contagem para a próxima party!
        }

        void CheckPartyFinished()
        {
            if (_currentParty.Count == 0)
                _partyActive = false;
        }
    }
}