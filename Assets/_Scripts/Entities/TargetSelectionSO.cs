using UnityEngine;

namespace DungeonKeeper
{
    public abstract class TargetSelectionSO : ScriptableObject
    {
        /// <summary>
        /// Método abstrato que cada estratégia vai implementar para escolher um alvo
        /// </summary>
        public abstract Character SelectTarget(Character self, Character[] potentialTargets);
    }
}