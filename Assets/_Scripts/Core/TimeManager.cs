using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public float RunTime  { get; private set; }
    public bool IsPaused  { get; private set; }

    public event Action OnPause;
    public event Action OnResume;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!IsPaused) RunTime += Time.deltaTime;
    }

    public void Pause()
    {
        IsPaused         = true;
        Time.timeScale   = 0f;
        OnPause?.Invoke();
    }

    public void Resume()
    {
        IsPaused         = false;
        Time.timeScale   = 1f;
        OnResume?.Invoke();
    }
}