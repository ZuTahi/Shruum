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
    private Animator animator;

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    public bool canDash = true;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    public bool canMove = true;
    public bool isInputGloballyLocked = false;

    public static PlayerMovement Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip walkClip;   // loopable walking sound
    public AudioClip dashClip;   // one-shot dash sound
    private AudioSource audioSource;

    [Header("Footstep Settings")]
    [Tooltip("Time between footsteps at normal walking speed")]
    public float baseStepInterval = 0.5f;
    private float stepTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        audioSource = PlayerStats.Instance.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!canMove)
        {
            // If player can't move, make Speed 0 in animator
            animator?.SetFloat("Speed", 0f);
            return;
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // ✅ Dash only if not interacting
        if (Input.GetKeyDown(KeyCode.Space)
            && canDash
            && !isDashing
            && dashCooldownTimer <= 0f
            && PlayerStats.Instance != null
            && PlayerStats.Instance.HasEnoughStamina(20))
        {
            isDashing = true;
            PlayerStats.Instance.SpendStamina(20);
            isInvincible = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            if (dashClip != null)
                audioSource.PlayOneShot(dashClip);

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

        if (isDashing)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime + Vector3.down * 0.1f);
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
                isInvincible = false;
            }

            return;
        }

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camRight * input.x + camForward * input.z).normalized;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        Vector3 finalMove = moveDir * currentSpeed * Time.deltaTime;
        finalMove += Vector3.down * 0.1f;
        controller.Move(finalMove);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }
        float normalizedSpeed = controller.velocity.magnitude / moveSpeed;
        animator?.SetFloat("Speed", normalizedSpeed);
        
        // ✅ Footstep timing
        HandleFootsteps(moveDir, isSprinting);
    }

      private void HandleFootsteps(Vector3 moveDir, bool isSprinting)
    {
        if (moveDir != Vector3.zero && controller.isGrounded)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                float interval = baseStepInterval / (isSprinting ? sprintMultiplier : 1f);
                stepTimer = interval;

                if (walkClip != null)
                {
                    audioSource.pitch = Random.Range(0.95f, 1.05f); // little variation
                    audioSource.PlayOneShot(walkClip, 0.01f);
                }
            }
        }
        else
        {
            stepTimer = 0f;
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
