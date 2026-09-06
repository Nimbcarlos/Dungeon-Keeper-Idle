using System.Collections;
using UnityEngine;

namespace DungeonKeeper
{
    public class UI_MonsterDisplay : MonoBehaviour
    {
        [Header("Configurações da Exibição")]
        [SerializeField] private Transform _displayParent; 
        [SerializeField] private float _targetHeightPixels = 120f; // Fallback caso não encontre RectTransform
        [Range(0.1f, 1f)]
        [SerializeField] private float _heightPercentage = 0.8f; // 🎯 Porcentagem desejada da altura do container (ex: 0.8 = 80%)
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
        /// Ajusta a escala do monstro com base na porcentagem (_heightPercentage) do container
        /// </summary>
        private void AdjustScaleToFitContainer(GameObject monsterInstance)
        {
            SpriteRenderer[] renderers = monsterInstance.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0) return;

            // Encontra os limites combinados do SpriteRenderer em World Units
            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            float currentSpriteHeight = combinedBounds.size.y;
            if (currentSpriteHeight <= 0) return;

            // Pega a altura do container em pixels
            float containerHeight = _targetHeightPixels;
            if (_displayParent is RectTransform rectTransform)
            {
                float rectH = rectTransform.rect.height;
                if (rectH > 0) containerHeight = rectH;
            }

            // 🎯 CÁLCULO BASEADO EM PORCENTAGEM (Sem divisão por 100 incorreta)
            // Converte a altura desejada em pixels para a proporção equivalente do Sprite
            float desiredHeightPixels = containerHeight * _heightPercentage;
            
            // Como 1 unidade no Canvas costuma equivaler a 1px quando ajustado, 
            // calculamos o fator multiplicador diretamente pela altura do Bounds:
            float scaleFactor = desiredHeightPixels / (currentSpriteHeight * 100f); 

            monsterInstance.transform.localScale = Vector3.one * scaleFactor;
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