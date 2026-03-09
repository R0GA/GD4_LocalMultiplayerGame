using System.Collections;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelNumber;
    [SerializeField] private GameObject GameEndText;
    [SerializeField] private GameObject EndScreen;
    [SerializeField] private string nextLevel;

    public void EndLevel()
    {
        StartCoroutine(EndLevelCoroutine());
    }
    private IEnumerator EndLevelCoroutine()
    {
        Time.timeScale = 0f;
        GameManager.Instance.player1.isDead = true;
        GameManager.Instance.player2.isDead = true; 
        GameEndText.SetActive(true);
        yield return new WaitForSecondsRealtime(7);
        EndScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(7);
        GameManager.Instance.LoadLevel(nextLevel);
    }
}
