using UnityEngine;
using DungeonKeeper;

// SkillPreview — usa CombatPoint
public class SkillPreview : MonoBehaviour
{
    [SerializeField] private VFXType  _vfxType;
    [SerializeField] private float    _cooldown = 3f;
    [SerializeField] private Character _character;

    private float _timer;

    void Awake()
    {
        if (_character == null)
            _character = GetComponent<Character>();
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = _cooldown;
            VFXManager.Instance.Play(_vfxType, _character.CombatPoint.position);
        }
    }
}
