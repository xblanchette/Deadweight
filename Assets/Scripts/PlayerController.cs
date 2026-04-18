using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    public float normalMoveSpeed = 6.0f;
    public float carryingMoveSpeed = 4.0f;

    public float rotationSpeedSmoothness = 5f;
    public float deadzone = 0.15f;
    public bool normalizeMoveInput = true;
    public Vector3 cameraOffset;
    public InputSystem_Actions playerControls;


    private Rigidbody rb;
    private Vector3 virtualInputForward;

    private Camera mainCamera;
    private PlayerInput playerInput;

    private Vector2 moveInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        virtualInputForward = transform.forward;
        playerControls = new InputSystem_Actions();
    }

    private void Start()
    {
        SetPlayerMap();
    }

    private void SetPlayerMap() {
        // Setup In game inputs

        playerControls.Player.Enable();

        playerControls.Player.Move.started += OnMove;
        playerControls.Player.Move.performed += OnMove;
        playerControls.Player.Move.canceled += OnMove;
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate() {
        HandleMovement();
        MoveCamera();
    }

    private void HandleMovement() {
        var move = new Vector3(moveInput.x, 0, moveInput.y);

        if (move.magnitude < deadzone) {
            NoMovement();
            return;
        }

        if (normalizeMoveInput) {
            move = move.normalized;
        }

        if (move.magnitude == 0) {
            NoMovement();
            return;
        }

        // Rotate the input vector to account for the camera
        move = Quaternion.Euler(0, 45, 0) * move;

        virtualInputForward = Vector3.RotateTowards(virtualInputForward, move, 1000f, 0f);

        var targetRot = Quaternion.LookRotation(virtualInputForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeedSmoothness * Time.fixedDeltaTime);

        var moveVector = normalMoveSpeed * move;
        rb.linearVelocity = moveVector;
    }

    private void NoMovement() {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    private void MoveCamera() {
        mainCamera.transform.position = transform.position + cameraOffset;
    }

}
