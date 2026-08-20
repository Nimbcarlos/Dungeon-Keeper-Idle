using UnityEngine;

namespace DungeonKeeper
{

    public class DamageTextManager : MonoBehaviour
    {
        public static DamageTextManager Instance { get; private set; }

        [SerializeField] private GameObject floatingTextPrefab;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // 1. Método Original (para dano numérico)
        public void SpawnDamageText(Vector3 spawnPosition, int damage, bool isCritical = false)
        {
            if (PiPUIVisibility.IsPiPActive) return;

            Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);
            GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition + randomOffset, Quaternion.identity);
            
            FloatingText ft = textObj.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Setup(damage, isCritical);
            }
        }

        // 2. Nova Sobrecarga (para XP e textos de sistema)
        public void SpawnDamageText(Vector3 spawnPosition, string message, Color textColor, float fontSize = 5f)
        {
            if (PiPUIVisibility.IsPiPActive) return;

            Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), 0, 0);
            GameObject textObj = Instantiate(floatingTextPrefab, spawnPosition + randomOffset, Quaternion.identity);
            
            FloatingText ft = textObj.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Setup(message, textColor, fontSize);
            }
        }
    }
}