using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    [Header("Control Schemes")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Player References")]
    [SerializeField] private CharacterController charControl;
    [SerializeField] private Transform otherPlayer;

    [Header("Player Settings")]
    [SerializeField] private bool isP1;
    [SerializeField] private float moveSpeed = 5f;
    //[SerializeField] private float lookSpeed = 50f;
    private Vector2 moveInput;
    private Vector3 lookInput;
    public float health = 100f;

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
        // Implement attack logic here
    }

public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Implement death logic here (e.g., disable player, play animation, etc.)
    }
}
