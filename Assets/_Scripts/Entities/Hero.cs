using UnityEngine;
using System.Collections;

namespace DungeonKeeper
{
    public class Hero : Character
    {
        public HeroData Data { get; private set; }

        // Mapeamos a Hash do parâmetro "State" (Integer) do Animator do Hero Editor
        private static readonly int StateHash = Animator.StringToHash("State");
        private static readonly int Victory = Animator.StringToHash("Victory");

        private int _currentStateValue = -1; // Cache para evitar chamadas redundantes ao Animator

        private enum CharacterState
        {
            Stand = 0,
            Walk = 1,
            Run = 2,
            Jump = 3,
            Crouch = 4,
            Climb = 5,
            DeathBack = 6
        }

        public bool HasSacked { get; set; }

        public void Initialize(HeroData heroData)
        {
            Data = heroData;
            base.Initialize(heroData.stats);
        }

        /// <summary>
        /// Define a animação pelo número do estado do Hero Editor, evitando requisições repetidas no mesmo frame
        /// </summary>
        public void SetState(int stateValue)
        {
            if (Animator == null) return;

            // Só envia para o Animator se o estado REALMENTE mudou!
            if (_currentStateValue == stateValue) return;

            _currentStateValue = stateValue;
            Animator.SetInteger(StateHash, stateValue);
        }

        protected override void OnAttack()
        {
            if (Animator == null) return;
            // Efeitos visuais ou sonoros adicionais ao atacar podem vir aqui
        }

        protected override void OnHit()
        {
            if (Animator == null) return;
            // Se o controller tiver a trigger de hurt:
            // Animator.SetTrigger("Hurt");
        }

        protected override void OnDieEffect()
        {
            if (Data == null) return;

            Treasure treasure = FindAnyObjectByType<Treasure>();
            if (treasure != null) treasure.AddGold(Data.goldReward);

            if (ResourceManager.Instance != null)
                ResourceManager.Instance.AddEssence(Data.essenceReward);

            if (ResourceManager.Instance != null)
                ResourceManager.Instance.GrantXPToActiveMonsters(Data._xpReward); // Atualiza o valor de XP fixo para os monstros

            Debug.Log($"Herói derrotado: +{Data.goldReward} Gold, +{Data.essenceReward} Essência");

            // Aciona o estado de Morte (DeathBack = 6 no Animator)
            SetState((int)CharacterState.DeathBack);
        }

        public override void Die()
        {
            OnDieEffect();

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Destroy(gameObject, 1.2f);
        }

        public void CelebrateVictoryAndDespawn(float victoryDuration = 1.5f)
        {
            if (Animator != null)
            {
                Debug.Log("Herói comemorando vitória!");
                SetState((int)CharacterState.Stand);
                Animator.SetBool(Victory, true);
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            StartCoroutine(VictoryRoutine(victoryDuration));
        }

        private IEnumerator VictoryRoutine(float duration)
        {
            Debug.Log($"Herói comemorando por {duration} segundos antes de desaparecer.");
            yield return new WaitForSeconds(duration);
            Destroy(gameObject);
        }
    }
}