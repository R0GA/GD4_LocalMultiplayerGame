using UnityEngine;

public class UITest : MonoBehaviour
{
  public void NextLevel()
    {
        GameManager.Instance.LoadLevel("LoadTest");
    }
}
