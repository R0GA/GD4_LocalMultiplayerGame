using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Control Schemes")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Player References")]
    [SerializeField] private CharacterController charControl;
    [SerializeField] private Transform otherPlayer;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform model;
    [SerializeField] private Material baseMat;
    [SerializeField] private Material hurtMat;
    [SerializeField] private Renderer modelRenderer;

    [Header("Player Settings")]
    [SerializeField] public bool isP1; // P1 = Fire Wizard, P2 = Lightning Wizard
    [SerializeField] public float moveSpeed = 5f;
    public float maxHealth = 100f;
    public float health = 100f;
    [SerializeField] private Animator healthAnim;
    public bool isDead = false;

    [Header("Attack Settings - Fire Wizard (P1)")]
    [SerializeField] private FireballProjectile fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballCooldown = 0.5f;

    [Header("Attack Settings - Lightning Wizard (P2)")]
    [SerializeField] private LightningAttack lightningAttack;

    private Vector2 moveInput;
    private Vector3 lookInput;
    private float lastFireballTime = -999f;
    private bool isAttacking;
    private float baseSpeed;

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button firstSelectedButton;

    public bool isPaused = false;

    private void Start()
    {
       // playerInput.SwitchCurrentActionMap("Character");
        baseSpeed = moveSpeed;

        playerInput.actions["Movement"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();

        playerInput.actions["Movement"].canceled += ctx => moveInput = Vector2.zero;
        playerInput.actions["Look"].canceled += ctx => lookInput = Vector2.zero;

        playerInput.actions["Attack"].performed += ctx => Attack();
        playerInput.actions["Pause"].performed -= OnPausePerformed;
        playerInput.actions["Pause"].performed += OnPausePerformed;
    }

    private void Update()
    {
        Movement();
        Look();
    }
    public void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (isPaused || otherPlayer.gameObject.GetComponent<PlayerController>().isPaused) ResumeGame();
        else PauseGame();
        Debug.Log($"Pause Button Pressed {isPaused} {otherPlayer.gameObject.GetComponent<PlayerController>().isPaused}");
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void Movement()
    {
        if (!isDead)
        {
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
            //move = transform.TransformDirection(move);
            charControl.Move(move * moveSpeed * Time.deltaTime);
            model.transform.position = transform.position;

            if (moveInput.sqrMagnitude > 0.1f)
            {
                Vector3 lookDir = new Vector3(moveInput.x, 0f, moveInput.y);
                model.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }

            if (!isAttacking && charControl.velocity.magnitude > 0.1f)
            {
                animator.SetInteger("AnimState", 1);
            }
            else if (!isAttacking)
            {
                animator.SetInteger("AnimState", 0);
            }
        }
    }

    public void Look()
    {
        if (lookInput.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = new Vector3(lookInput.x, 0f, lookInput.y);
            otherPlayer.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }

    public void Attack()
    {
        if (!isDead)
        {
            if (health <= 0) return;

            if (isP1)
            {
                FireWizardAttack();
                isAttacking = true;
                animator.SetInteger("AnimState", 2);

            }
            else
            {
                LightningWizardAttack();
                isAttacking = true;
                animator.SetInteger("AnimState", 2);
            }
        }
    }

    private void FireWizardAttack()
    {
        if (Time.time < lastFireballTime + fireballCooldown) return;
        if (fireballPrefab == null) return;

        Vector3 aimDirection = transform.forward;
        aimDirection.y = 0f;
        aimDirection.Normalize();

        Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position + Vector3.up * 0.5f;
        Quaternion spawnRot = Quaternion.LookRotation(aimDirection);

        FireballProjectile fb = Instantiate(fireballPrefab, spawnPos, spawnRot);
        fb.Launch(aimDirection);

        lastFireballTime = Time.time;
        StartCoroutine(ResetAttack());
    }

    private void LightningWizardAttack()
    {
        Debug.Log("Lightning Attack Pressed");
        if (lightningAttack == null) return;
        Debug.Log("Lightning Attack Pressed and not null");
        Vector3 aimDirection = transform.forward;
        aimDirection.y = 0f;
        aimDirection.Normalize();

        lightningAttack.Fire(transform.position, aimDirection);
        StartCoroutine(ResetAttack());
    }

    public void TakeDamage(float damage)
    {
        if(isDead) return;

        StartCoroutine(FlashHurtMaterial());
        health -= damage;
        UpdateHealthUI();

        if (health <= 0)
            Die();
    }
    void UpdateHealthUI()
    {
        var healthPercentage = health / maxHealth;

        if (healthPercentage > 0.66)
        {
            healthAnim.SetInteger("AnimState", 0);
        }
        else if (healthPercentage > 0.33)
        {
            healthAnim.SetInteger("AnimState", 1);
        }
        else
        {
            healthAnim.SetInteger("AnimState", 2);
        }
    }
    private IEnumerator FlashHurtMaterial()
    {
        modelRenderer.material = hurtMat;
        yield return new WaitForSeconds(0.2f);
        modelRenderer.material = baseMat;
    }
    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died.");
        animator.SetInteger("AnimState", 0);
        model.gameObject.transform.rotation = Quaternion.Euler(90f, model.rotation.eulerAngles.y, model.rotation.eulerAngles.z);

        if (isDead && otherPlayer.gameObject.GetComponent<PlayerController>().isDead)
        {
            GameManager.Instance.ReloadLevel();
        }
    }
    public void levelReset()
    {
        Time.timeScale = 1f;
        isPaused = false;
        playerInput.SwitchCurrentActionMap("Character");
        model.transform.rotation = Quaternion.identity;
        animator.SetInteger("AnimState", 0);
        moveSpeed = baseSpeed;
        health = maxHealth;
        isDead = false;
        charControl.enabled = false;
        Vector3 randomOffset = Random.insideUnitSphere * 1f;
        randomOffset.y = 1f;
        transform.position = randomOffset;
        charControl.enabled = true;
        UpdateHealthUI();
    }
}