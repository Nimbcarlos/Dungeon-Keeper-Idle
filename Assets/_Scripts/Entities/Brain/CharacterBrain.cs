using UnityEngine;

namespace DungeonKeeper
{
    [RequireComponent(typeof(Character))]
    public abstract class CharacterBrain : MonoBehaviour
    {
        protected Character character;

        // 🎯 O eixo X usa o pé (para o alcance). O eixo Y usa a raiz do objeto para 
        // ignorar o pulo da animação de corrida (que causava falsas mudanças de Y).
        public Vector2 MyFeetPos 
        {
            get 
            {
                float xPos = character.FeetPoint != null ? character.FeetPoint.position.x : transform.position.x;
                float yPos = transform.position.y; 
                return new Vector2(xPos, yPos);
            }
        }

        protected virtual void Awake()
        {
            character = GetComponent<Character>();
        }

        protected virtual void Update()
        {
            if (character == null || !character.IsAlive) return;
            Think();
        }

        protected abstract void Think();

        public Vector2 GetEntityFeetPos(Character entity)
        {
            float xPos = entity.FeetPoint != null ? entity.FeetPoint.position.x : entity.transform.position.x;
            float yPos = entity.transform.position.y;
            return new Vector2(xPos, yPos);
        }

        // 🎯 O FIX DO PING-PONG: Dead Zone para alinhar as Lanes suavemente
        protected Vector2 CalculateTwoPhaseMovement(Vector2 currentPos, Vector2 targetPos)
        {
            float deltaY = targetPos.y - currentPos.y;
            float deltaX = targetPos.x - currentPos.x;

            const float yTolerance = 0.08f;
            const float xTolerance = 0.05f;

            // Primeiro alinha verticalmente (respeitando a zona morta)
            if (Mathf.Abs(deltaY) > yTolerance)
                return new Vector2(0f, Mathf.Sign(deltaY));

            // Depois avança horizontalmente
            if (Mathf.Abs(deltaX) > xTolerance)
                return new Vector2(Mathf.Sign(deltaX), 0f);

            return Vector2.zero;
        }

        protected void ApplyVisualFlip(float directionX)
        {
            if (Mathf.Abs(directionX) < 0.01f) return;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(directionX) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        protected virtual void ExecuteMovement(Vector2 moveDirection, float lookDirectionX)
        {
            ApplyVisualFlip(lookDirectionX);

            if (moveDirection == Vector2.zero)
            {
                character.Animator?.SetBool("isMoving", false);
                character.Move(Vector2.zero);
                return;
            }

            character.Animator?.SetBool("isMoving", true);
            character.Move(moveDirection);
        }

        // Sistema genérico de busca (Radar)
        public T[] GetAliveEntities<T>() where T : Character
        {
            T[] allEntities = FindObjectsByType<T>(FindObjectsInactive.Exclude);
            return System.Array.FindAll(allEntities, entity => entity != null && entity.IsAlive);
        }
    }
}