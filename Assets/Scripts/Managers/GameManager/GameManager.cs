using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Ending
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void DisableDebugLoggingInBuild()
    {
#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif
    }

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif
    }

    private void Start()
    {
        SetGameState(GameState.Playing);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartAmbient();
        }
    }

    public void SetGameState(GameState newState)
    {
        CurrentState = newState;

        switch (CurrentState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.StopAmbient();
                }
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.Ending:
                Time.timeScale = 1f;
                break;
        }

        Debug.Log($"Game State Changed : {CurrentState}");
    }
}