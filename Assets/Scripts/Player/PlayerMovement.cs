using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Invincibility")]
    public bool isInvincible = false;

    private CharacterController controller;

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    public bool canMove = true;

    public static PlayerMovement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!canMove) return; // ✅ prevent movement when disabled

        // DASH COOLDOWN TIMER
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // START DASH
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCooldownTimer <= 0f && PlayerStats.Instance.HasEnoughStamina(20))
        {
            isDashing = true;
            PlayerStats.Instance.SpendStamina(20);
            isInvincible = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            // Get input in camera-relative direction
            Vector3 dashInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            Vector3 dashCamForward = Camera.main.transform.forward;
            Vector3 dashCamRight = Camera.main.transform.right;
            dashCamForward.y = 0f;
            dashCamRight.y = 0f;
            dashCamForward.Normalize();
            dashCamRight.Normalize();

            Vector3 dashIsoDir = (dashCamRight * dashInput.x + dashCamForward * dashInput.z).normalized;
            dashDirection = dashIsoDir == Vector3.zero ? transform.forward : dashIsoDir;
        }

        // DASH MOVEMENT
        if (isDashing)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime + Vector3.down * 0.1f);
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
                isInvincible = false;
            }

            return; // skip regular movement while dashing
        }

        // REGULAR MOVEMENT
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camRight * input.x + camForward * input.z).normalized;

        // Sprint check
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // Apply movement
        Vector3 finalMove = moveDir * currentSpeed * Time.deltaTime;
        finalMove += Vector3.down * 0.1f; // stick to ground
        controller.Move(finalMove);

        // Rotate player toward movement
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }
    }

    public void TemporarilyLockMovement(float duration)
    {
        StartCoroutine(LockMovementRoutine(duration));
    }

    private IEnumerator LockMovementRoutine(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

}
