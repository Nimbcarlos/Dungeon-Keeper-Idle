using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class UI_MonsterDisplay : MonoBehaviour
    {
        [Header("Configurações da Exibição")]
        [SerializeField] private Transform _displayParent; 
        [SerializeField] private float _targetHeightPixels = 120f; // Altura máxima desejada para o monstro no container
        [SerializeField] private Vector3 _spawnOffset = new Vector3(0f, 0f, 0f);

        private GameObject _currentMonsterInstance;
        private Animator _currentAnimator;
        private Coroutine _behaviorCoroutine;

        public void DisplayMonster(MonsterData monsterData)
        {
            ClearDisplay();

            if (monsterData == null || monsterData.prefab == null) return;

            Transform parent = _displayParent != null ? _displayParent : transform;

            _currentMonsterInstance = Instantiate(monsterData.prefab, parent);
            
            // 🎯 AJUSTE DINÂMICO DE ESCALA
            AdjustScaleToFitContainer(_currentMonsterInstance);

            _currentMonsterInstance.transform.localPosition = _spawnOffset;

            // Desenha na frente de toda a UI
            SpriteRenderer[] renderers = _currentMonsterInstance.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers)
            {
                sr.sortingLayerName = "UI";
                sr.sortingOrder = 1000;
            }

            // Desativa inteligência artificial e colisão do monstro
            MonsterBrain brain = _currentMonsterInstance.GetComponent<MonsterBrain>();
            if (brain != null) brain.enabled = false;

            Monster monsterComp = _currentMonsterInstance.GetComponent<Monster>();
            if (monsterComp != null) monsterComp.enabled = false;

            Collider2D col = _currentMonsterInstance.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Suporte para rodar animações com tempo pausado
            _currentAnimator = _currentMonsterInstance.GetComponent<Animator>();
            if (_currentAnimator != null)
            {
                _currentAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            _behaviorCoroutine = StartCoroutine(RandomBehaviorRoutine());
        }

        /// <summary>
        /// Calcula os limites (Bounds) de todos os SpriteRenderers do monstro e ajusta a escala
        /// </summary>
        private void AdjustScaleToFitContainer(GameObject monsterInstance)
        {
            SpriteRenderer[] renderers = monsterInstance.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0) return;

            // Encontra os limites combinados do SpriteRenderer
            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            float currentSpriteHeight = combinedBounds.size.y;
            if (currentSpriteHeight <= 0) return;

            // Descobre o tamanho do container de UI se existir
            float containerHeight = _targetHeightPixels;
            if (_displayParent is RectTransform rectTransform)
            {
                containerHeight = rectTransform.rect.height;
            }

            // Calcula a escala proporcional para o monstro ocupar 80% do container
            float desiredHeight = containerHeight * 0.6f;
            float scaleFactor = desiredHeight / (currentSpriteHeight * 100f); 

            monsterInstance.transform.localScale = Vector3.one * Mathf.Clamp(scaleFactor, 0.1f, 100f);
        }

        public void ClearDisplay()
        {
            if (_behaviorCoroutine != null)
            {
                StopCoroutine(_behaviorCoroutine);
                _behaviorCoroutine = null;
            }

            if (_currentMonsterInstance != null)
            {
                Destroy(_currentMonsterInstance);
                _currentMonsterInstance = null;
            }
            _currentAnimator = null;
        }

        private IEnumerator RandomBehaviorRoutine()
        {
            while (_currentAnimator != null)
            {
                yield return new WaitForSecondsRealtime(Random.Range(2f, 4f));

                if (_currentAnimator == null) break;

                int randomAction = Random.Range(0, 3);
                switch (randomAction)
                {
                    case 0:
                        _currentAnimator.SetBool("isMoving", false);
                        break;
                    case 1:
                        _currentAnimator.SetBool("isMoving", true);
                        break;
                    case 2:
                        _currentAnimator.SetBool("isMoving", false);
                        _currentAnimator.SetTrigger("attack");
                        break;
                }
            }
        }

        private void OnDisable()
        {
            ClearDisplay();
        }
    }
}