using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tablet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StartScript startScript;
    [SerializeField] private Sprite litImage;
    [SerializeField] private Image floorImage;
    [SerializeField] private Image wallImage;
    [SerializeField] public bool lit;
    [SerializeField] private bool isPlayer1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if (isPlayer1 && other.GetComponent<PlayerController>().isP1)
            {
                wallImage.sprite = litImage;
                floorImage.sprite = litImage;
                lit = true;
            }
            else if (!isPlayer1 && !other.GetComponent<PlayerController>().isP1)
            {
                wallImage.sprite = litImage;
                floorImage.sprite = litImage;
                lit = true;
            }
        } 
    }
}
