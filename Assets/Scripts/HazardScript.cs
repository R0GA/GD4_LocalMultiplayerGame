using System.Collections.Generic;
using UnityEngine;

public class HazardScript : MonoBehaviour
{
    [Header("Hazard Settings")]
    [SerializeField] private float slowAmount = 2f;
    [SerializeField] private bool isLethal = false;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageSpeed = 1f;
    private float lastDamageTime;

    private List<PlayerController> playersInside = new List<PlayerController>();

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !playersInside.Contains(player))
        {
            playersInside.Add(player);
            player.moveSpeed -= slowAmount;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && playersInside.Contains(player))
        {
            playersInside.Remove(player);
            player.moveSpeed += slowAmount;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isLethal) return;

        if (Time.time >= lastDamageTime + 1f / damageSpeed)
        {
            foreach (PlayerController player in playersInside)
            {
                player.TakeDamage(damage);
            }
            lastDamageTime = Time.time;
        }
    }
}