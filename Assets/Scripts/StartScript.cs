using UnityEngine;

public class StartScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tablet p1FLoor;
    [SerializeField] private Tablet p2FLoor;
    [SerializeField] private LevelManager levelManager;
    private bool p1Lit;
    private bool p2Lit;
    bool triggeredEnd = false;


    private void Update()
    {
        p1Lit = p1FLoor.lit;
        p2Lit = p2FLoor.lit;

        if (p1Lit && p2Lit && !triggeredEnd)
        {
            triggeredEnd = true;
            levelManager.EndLevel();
        }
    }

}
