using UnityEngine;

namespace DungeonKeeper
{
    public abstract class TargetSelectionSO : ScriptableObject
    {
        /// <summary>
        /// Dado um originador e um array de alvos potenciais (Monstros, Invocações, Armadilhas),
        /// retorna o Character ideal segundo a regra desta estratégia.
        /// </summary>
        public abstract Character SelectTarget(Character self, Character[] potentialTargets);
    }
}