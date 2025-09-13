using UnityEngine;
using System.Collections;

public class SlingShotWeapon : ModularWeaponCombo
{
    [Header("Basic Settings")]
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public float projectileSpeed = 20f;
    public int manaCost = 30;
    [Header("Audio Clips")]
    public AudioClip attackClip;
    public AudioClip finisherClip;

    // Mix finishers
    public AudioClip OrbitalMixClip;
    public AudioClip VinStrikeMixClip;
    private AudioSource audioSource;
    [Header("Projectile Prefabs")]
    public GameObject seedProjectilePrefab;

    [Header("Mix Finishers")]
    public GameObject orbitalSicklePrefab; // LLJ

    [Header("Vine Strike Settings")]
    public GameObject vineStrikePrefab;
    public int vineStrikeCount = 6;        // how many spikes in the wave
    public float vineStrikeSpacing = 1.5f; // distance between spikes
    public float vineStrikeDelay = 0.1f;

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private float nextAttackAllowedTime = 0f;

    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
        suppressInput = true; // Disable input until equipped

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.9f;
    }

    void Update()
    {
        if (suppressInput) return;

        if (comboStep > 0 && Time.time - lastAttackTime > comboResetDelay)
        {
            Debug.Log("[Slingshot] Combo timed out, resetting.");
            ResetCombo();
        }
    }

    public void EnableInputWithDelay(float delay)
    {
        StartCoroutine(EnableInputAfterDelay(delay));
    }

    private IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        suppressInput = false;
        Debug.Log("[Slingshot] Input re-enabled after equip delay.");
    }

    public override void HandleInput()
    {
        if (suppressInput) return;
        if (Time.time < nextAttackAllowedTime) return; // Prevent spam

        var currentState = PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsTag("Sling"))
        {
            if (PlayerAnimationHandler.Instance.animator.IsInTransition(0) ||
                currentState.normalizedTime < 0.85f)
                return;
        }

        ProcessAttack();
    }

    private void ProcessAttack()
    {
        lastAttackTime = Time.time;

        if (comboStep >= 3)
            comboStep = 0; // Always restart after third attack

        comboStep++;

        Debug.Log($"[Slingshot] Playing Attack {comboStep}");

        PlayerAnimationHandler.Instance.StopAttackAnimation();
        PlayerAnimationHandler.Instance.PlayAttackAnimation(3, comboStep, this);

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.TemporarilyLockMovement(0.1f);

        if (comboStep == 3 && !suppressNormalFinisher)
        {
            // Scatter shot handled in SpawnFinisherVFX()
        }

        suppressInput = true;
        StartCoroutine(EnableInputAfterAnimation());
    }

    private IEnumerator EnableInputAfterAnimation()
    {
        var anim = PlayerAnimationHandler.Instance.animator;

        while (anim.GetCurrentAnimatorStateInfo(0).IsTag("Sling") &&
               anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        suppressInput = false;
    }

    // Animation Event — Normal shot spawn
    public override void SpawnAttackVFX()
    {
        PlayAttackSound();
        if (comboStep < 3)
        {
            GameObject proj = Instantiate(seedProjectilePrefab, attackPoint.position, attackPoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = attackPoint.forward * projectileSpeed;
        }
    }

    // Animation Event — Finisher spawn
    public override void ExecuteFinisher()
    {
        PlayFinisherSound();
        if (comboStep == 3 && !suppressNormalFinisher)
        {
            // Scatter shot (normal finisher)
            float[] angles = { -15f, 0f, 15f };
            foreach (float angle in angles)
            {
                Quaternion rot = Quaternion.Euler(0, angle, 0) * attackPoint.rotation;
                GameObject proj = Instantiate(seedProjectilePrefab, attackPoint.position, rot);
                if (proj.TryGetComponent<Rigidbody>(out var rb))
                    rb.linearVelocity = rot * Vector3.forward * projectileSpeed;
            }
            ResetCombo();
        }
    }

    public override void DoHitDetection()
    {
        // Slingshot is projectile only
    }

    public override void HandleMixFinisher(ModularWeaponInput[] combo)
    {
        if (combo == null || combo.Length < 3 || slotManager == null) return;

        var weapons = slotManager.GetAllWeapons();
        int i0 = (int)combo[0];
        int i1 = (int)combo[1];
        int i2 = (int)combo[2];

        if (i0 < 0 || i0 >= weapons.Length ||
            i1 < 0 || i1 >= weapons.Length ||
            i2 < 0 || i2 >= weapons.Length)
            return;

        ModularWeaponCombo w1 = weapons[i0];
        ModularWeaponCombo w2 = weapons[i1];
        ModularWeaponCombo w3 = weapons[i2];

        if (w1 is DaggerCombo && w2 is DaggerCombo && w3 is SlingShotWeapon)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);

            var sickle = Instantiate(orbitalSicklePrefab, transform.root.position, Quaternion.identity);
            if (sickle.TryGetComponent<OrbitingSicklesController>(out var orbital))
                orbital.followTarget = transform.root;
            PlayMixFinisherSound(OrbitalMixClip);     
            ResetCombo();
            suppressNormalFinisher = true;
            FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
        }
        else if (w1 is GauntletCombo && w2 is GauntletCombo && w3 is SlingShotWeapon)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);

            StartCoroutine(SpawnVineStrikeWave());

            ResetCombo();
            suppressNormalFinisher = true;
            FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
        }
    }

    private IEnumerator SpawnVineStrikeWave()
    {
        PlayMixFinisherSound(VinStrikeMixClip);
        Vector3 startPos = attackPoint.position;
        Vector3 dir = attackPoint.forward;

        for (int i = 0; i < vineStrikeCount; i++)
        {
            Vector3 spawnPos = startPos + dir * (i * vineStrikeSpacing);
            spawnPos.y = 1f;
            Instantiate(vineStrikePrefab, spawnPos, Quaternion.LookRotation(dir));
            yield return new WaitForSeconds(vineStrikeDelay);
        }
    }

    public override void ResetCombo()
    {
        comboStep = 0;
        suppressNormalFinisher = false;
        suppressInput = false;

        PlayerAnimationHandler.Instance.StopAttackAnimation();
        PlayerAnimationHandler.Instance.animator.SetInteger("AttackIndex", 0);
    }

    private void PlayAttackSound()
    {
        if (attackClip != null) audioSource.PlayOneShot(attackClip);
    }

    private void PlayFinisherSound()
    {
        if (finisherClip != null) audioSource.PlayOneShot(finisherClip);
    }

    private void PlayMixFinisherSound(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }
    }
