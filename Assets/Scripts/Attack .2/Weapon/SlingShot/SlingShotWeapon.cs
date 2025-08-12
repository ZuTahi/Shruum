using UnityEngine;
using System.Collections;

public class SlingShotWeapon : ModularWeaponCombo
{
    [Header("Basic Settings")]
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public float projectileSpeed = 20f;
    public int manaCost = 30;

    [Header("Projectile Prefabs")]
    public GameObject seedProjectilePrefab;
    public GameObject scatterSeedPrefab;

    [Header("Finisher Settings")]
    public GameObject explosiveSeedPrefab; // Normal finisher (LLK)
    public float explosiveSeedSpeed = 18f;

    [Header("Mix Finishers")]
    public GameObject orbitalSicklePrefab; // JJK
    public GameObject vineStrikePrefab;    // KKJ

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private float nextAttackAllowedTime = 0f;

    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
        suppressInput = true; // Disable input until equipped
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
            // Scatter shot or explosive seed handled in SpawnFinisherVFX()
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
        if (comboStep < 3)
        {
            GameObject proj = Instantiate(seedProjectilePrefab, attackPoint.position, attackPoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = attackPoint.forward * projectileSpeed;
        }
    }

    // Animation Event — Finisher spawn
    public override void SpawnFinisherVFX()
    {
        if (comboStep == 3 && !suppressNormalFinisher)
        {
            // Scatter shot (normal finisher)
            float[] angles = { -15f, 0f, 15f };
            foreach (float angle in angles)
            {
                Quaternion rot = Quaternion.Euler(0, angle, 0) * attackPoint.rotation;
                GameObject proj = Instantiate(scatterSeedPrefab, attackPoint.position, rot);
                if (proj.TryGetComponent<Rigidbody>(out var rb))
                    rb.linearVelocity = rot * Vector3.forward * projectileSpeed;
            }
            ResetCombo();
        }
        else if (explosiveSeedPrefab != null)
        {
            // Explosive seed
            GameObject proj = Instantiate(explosiveSeedPrefab, attackPoint.position, attackPoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = attackPoint.forward * explosiveSeedSpeed;

            Debug.Log("[Slingshot VFX] Spawned explosive seed finisher");
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

            ResetCombo();
            suppressNormalFinisher = true;
            FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
        }
        else if (w1 is GauntletCombo && w2 is GauntletCombo && w3 is SlingShotWeapon)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);

            Instantiate(vineStrikePrefab, attackPoint.position, transform.rotation);

            ResetCombo();
            suppressNormalFinisher = true;
            FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
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
}
