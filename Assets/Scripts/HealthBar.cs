using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public PlayerController PlayerController;
    public Image healthBarFill;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealthBar(float health)
    {
        float fillPercentage = health / maxHealth;
        healthBarFill.fillAmount = fillPercentage;

    }
}
