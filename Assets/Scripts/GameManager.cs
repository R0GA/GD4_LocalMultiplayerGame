using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Settings")]
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;
    private string currentLevel = "MainMenu";
    private GameObject endUi;

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
        LoadLevel("MainMenu");
    }
    public void EndLevel()
    {
        Time.timeScale = 0.0f;
        endUi.SetActive(true);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        endUi = GameObject.FindGameObjectWithTag("EndUI");
        endUi.SetActive(false);
    }
}
