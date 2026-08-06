using UnityEngine;

[RequireComponent(typeof(Character))]
public abstract class CharacterBrain : MonoBehaviour
{
    protected Character character;

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
}