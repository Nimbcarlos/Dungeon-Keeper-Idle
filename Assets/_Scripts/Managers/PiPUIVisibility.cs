using UnityEngine;

public class PiPUIVisibility : MonoBehaviour
{
    public static bool IsPiPActive { get; private set; } = false;

    // Evento para notificar a UI e os alvos quando o modo muda
    public delegate void OnPiPModeChanged(bool inPiP);
    public static event OnPiPModeChanged OnPiPStateChanged;

    void Awake()
    {
        Application.runInBackground = true;
    }

    // Chamado pelo Android/Unity quando altera o foco ou entra em PiP
    void OnApplicationPause(bool pauseStatus)
    {
        // Se a aplicação perdeu o foco, consideramos estado de background/PiP
        SetPiPState(pauseStatus);
    }

    public static void SetPiPState(bool active)
    {
        if (IsPiPActive == active) return;
        IsPiPActive = active;
        OnPiPStateChanged?.Invoke(IsPiPActive);
    }
}