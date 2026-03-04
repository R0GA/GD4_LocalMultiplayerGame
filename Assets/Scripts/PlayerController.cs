using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Control Schemes")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Player References")]
    [SerializeField] private CharacterController charControl;
    [SerializeField] private Transform otherPlayer;

    [Header("Player Settings")]
    [SerializeField] private bool isP1; // P1 = Fire Wizard, P2 = Lightning Wizard
    [SerializeField] private float moveSpeed = 5f;
    public float health = 100f;

    [Header("Attack Settings - Fire Wizard (P1)")]
    [SerializeField] private FireballProjectile fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballCooldown = 0.5f;

    [Header("Attack Settings - Lightning Wizard (P2)")]
    [SerializeField] private LightningAttack lightningAttack;

    private Vector2 moveInput;
    private Vector3 lookInput;
    private float lastFireballTime = -999f;

    private void Start()
    {
        playerInput.actions["Movement"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();

        playerInput.actions["Movement"].canceled += ctx => moveInput = Vector2.zero;
        playerInput.actions["Look"].canceled += ctx => lookInput = Vector2.zero;

        playerInput.actions["Attack"].performed += ctx => Attack();
    }

    private void Update()
    {
        Movement();
        Look();
    }

    public void Movement()
    {
        Vector3 move = new Vector3(-moveInput.y, 0, moveInput.x);
        move = transform.TransformDirection(move);
        charControl.Move(move * moveSpeed * Time.deltaTime);
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
        if (health <= 0) return;

        if (isP1)
            FireWizardAttack();
        else
            LightningWizardAttack();
    }

    private void FireWizardAttack()
    {
        if (Time.time < lastFireballTime + fireballCooldown) return;
        if (fireballPrefab == null) return;

        Vector3 aimDirection = transform.forward;
        aimDirection.y = 0f;
        aimDirection.Normalize();

        Vector3 spawnPos = fireballSpawnPoint != null ? fireballSpawnPoint.position : otherPlayer.position + Vector3.up * 0.5f;
        Quaternion spawnRot = Quaternion.LookRotation(aimDirection);

        FireballProjectile fb = Instantiate(fireballPrefab, spawnPos, spawnRot);
        fb.Launch(aimDirection);

        lastFireballTime = Time.time;
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
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
    }

    private void Die()
    {
        //Add death stuff Rowynn, dont be lazy
        Debug.Log($"{gameObject.name} has died.");
    }
}