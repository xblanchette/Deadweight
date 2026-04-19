using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    public GameObject visualObject;
    public Animator visualAnimator;
    public float time;

    public AudioSource walkSound;
    public bool isWalking = false;

    public float normalMoveSpeed = 6.0f;
    public float carryingMoveSpeed = 4.0f;

    public float rotationSpeedSmoothness = 5f;
    public float carryingRotationSpeedSmoothness = 1f;
    public float deadzone = 0.15f;
    public bool normalizeMoveInput = true;
    public Vector3 cameraOffset;

    [HideInInspector]
    public bool isCarryingSomething = false;

    [HideInInspector]
    public InputSystem_Actions playerControls;

    private Rigidbody rb;
    private Vector3 virtualInputForward;

    private Camera mainCamera;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private GameObject grabIndicator;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        virtualInputForward = transform.forward;
        playerControls = new InputSystem_Actions();
        grabIndicator = FindObjectsByType<GrabIndicator>(FindObjectsSortMode.None).FirstOrDefault().gameObject;
    }

    private void Start() {
        SetPlayerMap();
        SoundManager.instance.PlaySound(walkSound);
        StartCoroutine(Delay());
        visualAnimator.Play("Idle");
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

        if (grabIndicator != null) {
            grabIndicator.SetActive(!isCarryingSomething);
        }
    }

    private void HandleMovement() {
        var move = new Vector3(moveInput.x, 0, moveInput.y);

        // You can never have upward momentum
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0), rb.linearVelocity.z);

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

        if (isCarryingSomething) {
            // When you are carrying something, you are walking backwards
            targetRot = Quaternion.Euler(0, 180, 0) * targetRot;
        }

        var targetRotationSmoothness = isCarryingSomething ? carryingRotationSpeedSmoothness : rotationSpeedSmoothness;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, targetRotationSmoothness * Time.fixedDeltaTime);

        var targetSpeed = isCarryingSomething ? carryingMoveSpeed : normalMoveSpeed;

        var moveVector = (isCarryingSomething ? -1 : 1) * targetSpeed * transform.forward;
        rb.linearVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);

        visualAnimator.SetBool("isWalking", true);

        if (!isWalking) {
            isWalking = true;
            SoundManager.instance.UnPauseSound(walkSound);
        }
    }

    private void NoMovement() {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        isWalking = false;
        SoundManager.instance.PauseSound(walkSound);
        visualAnimator.SetBool("isWalking",false);
    }

    private void MoveCamera() {
        mainCamera.transform.position = transform.position + cameraOffset;
    }

    public IEnumerator Delay() {
        if (SceneManager.GetActiveScene().name == "Tuto")
        {
            visualAnimator.SetBool("isDragging", true);
            yield return new WaitForSeconds(time);
            visualAnimator.SetBool("isDragging", false);
            GetComponent<Animator>().enabled = false;
        }

    }

}
