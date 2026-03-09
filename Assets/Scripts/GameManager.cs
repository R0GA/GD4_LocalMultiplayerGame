using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Settings")]
    [SerializeField] public PlayerController player1;
    [SerializeField] public PlayerController player2;
    private string currentLevel = "MainMenu";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadLevel(string levelName)
    {
        currentLevel = levelName;
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
        player1.levelReset();
        player2.levelReset();
        Time.timeScale = 1.0f;
    }
    public void ReloadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentLevel);
        player1.levelReset();
        player2.levelReset();
        Time.timeScale = 1.0f;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ReturnToMainMenu()
    {
        LoadLevel("Start");
    }
    public void EndLevel()
    {
        Time.timeScale = 0.0f;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevel = scene.name;
    }
    public void UnPause()
    {
        if (!player1.isPaused && !player2.isPaused) return;

        if(player1.isPaused)
        {
            player1.ResumeGame();
        }
        else
        {
            player2.ResumeGame();
        }
    }
}
