using UnityEngine;

public class AndroidPiP : MonoBehaviour
{
    private AndroidJavaObject _activity;

    void Awake()
    {
        // 1. OBRIGATÓRIO: Impede que o loop do C# e da engine congele
        // quando a janela do app perde o foco principal no Android
        Application.runInBackground = true;
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try 
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao obter Activity: {e.Message}");
        }
#endif
    }

    /// <summary>
    /// Chama o método Java para acionar o PiP.
    /// É preferível chamar este método via botão da UI ou ação direta do jogador.
    /// </summary>
    public void EnterPiP()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_activity != null)
        {
            try
            {
                _activity.Call("triggerPiP");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao acionar PiP no C#: {e.Message}");
            }
        }
#endif
    }

    // 2. ATENÇÃO: Se você quer que entre em PiP automaticamente ao minimizar o app no botão Home/Gestos,
    // NÃO use OnApplicationPause no C#.
    // Quem deve fazer isso automaticamente é o método Java 'onUserLeaveHint()', que você já colocou no PiPPlugin.java!
}