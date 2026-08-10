using UnityEngine;
using System.Collections; // <-- ADICIONE ESTA LINHA SE NÃO ESTIVER LÁ

namespace DungeonKeeper
{
    public class Hero : Character
    {
        public HeroData Data { get; private set; }

        // Mapeamos a Hash do parâmetro "State" (Integer) do Animator do Hero Editor
        private static readonly int StateHash = Animator.StringToHash("State");
        private static readonly int Victory = Animator.StringToHash("Victory");

        // Enum opcional para deixar o código bem legível e organizado
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
        /// Define a animação pelo número do estado do Hero Editor
        /// </summary>
        public void SetState(int stateValue)
        {
            if (Animator == null) return;
            Animator.SetInteger(StateHash, stateValue);
        }

        protected override void OnAttack()
        {
            if (Animator == null) return;
            
            // Quando ataca, se houver um estado de ataque no Animator ou Trigger:
            // Por exemplo, podemos resetar o estado de corrida para Stand/Attack:
            SetState((int)CharacterState.Stand);
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
                // Para de andar/atacar e ativa a comemoração
                if (Animator != null)
                {
                    Debug.Log("Herói comemorando vitória!");
                    Animator.SetInteger(StateHash, 0); // Stand
                    Animator.SetBool(Victory, true); // Ativa a animação de Vitória
                }

                // Desativa colisão para não interferir em nada na sala
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                // Espera a comemoração e desaparece
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