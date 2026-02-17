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
    [SerializeField] private float lookSpeed = 50f;
    private Vector2 moveInput;
    private Vector3 lookInput;

    private void Start()
    {
        playerInput.actions["Movement"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();

        playerInput.actions["Movement"].canceled += ctx => moveInput = Vector2.zero;
        playerInput.actions["Look"].canceled += ctx => lookInput = Vector2.zero;
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
        float lookX = lookInput.x * lookSpeed * Time.deltaTime;
        otherPlayer.Rotate(0, lookX, 0);
    }
}
