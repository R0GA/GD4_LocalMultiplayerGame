using UnityEngine;

public class HazardScript : MonoBehaviour
{
    [Header("Hazard Settings")]
    [SerializeField] private float slowAmount = 2f;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerController>().moveSpeed -= slowAmount;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerController>().moveSpeed += slowAmount;
        }
    }

}
