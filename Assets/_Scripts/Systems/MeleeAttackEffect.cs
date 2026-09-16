using UnityEngine;

namespace DungeonKeeper
{
    /// <summary>Plays a single melee effect and releases its instance after the clip.</summary>
    public sealed class MeleeAttackEffect : MonoBehaviour
    {
        [Min(0.01f)] public float lifetime = 0.5f;

        private void OnEnable()
        {
            Destroy(gameObject, Mathf.Max(0.01f, lifetime));
        }
    }
}
