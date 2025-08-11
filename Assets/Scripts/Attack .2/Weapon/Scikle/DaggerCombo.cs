using UnityEngine;
using System.Collections;

public class DaggerCombo : ModularWeaponCombo
{
    [Header("Basic Combo")]
    public float fixedAttackDelay = 0.3f;
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public GameObject slashRedPrefab;
    public GameObject slashBluePrefab;
    public GameObject finisherPrefab;
    public float attackDamage = 12f;

    [Header("Mix Finisher: BladeBurst")]
    public GameObject bladeBurstPrefab;
    [Header("Mix Finisher: TripleBoomerang")]
    public GameObject tripleBoomerangPrefab;
    public int manaCost = 10;

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private bool isFinisherActive = false;

    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        suppressInput = true;
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
    }
    void Update()
    {
        if (suppressInput) return;
        if (comboStep > 0 && Time.time - lastAttackTime > comboResetDelay)
        {
            Debug.Log("[Dagger] Combo timed out, resetting.");
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
        Debug.Log("[Dagger] Input re-enabled after equip delay.");
    }

    public override void HandleInput()
    {
        if (suppressInput) return;

        // Check if animation is still playing and block input if so
        var currentState = PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsTag("Dagger") && currentState.normalizedTime < 1f)
        {
            Debug.Log("[InputHandler] Animation still playing, blocking input.");
            return; // Don't process input if the animation is still playing
        }

        // If it's time to process a new combo step, execute the attack
        ProcessAttack();
    }

    private void ProcessAttack()
    {
        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        Debug.Log($"[Attack] Playing Attack Animation -> AttackIndex: {comboStep}, WeaponType: {weaponType}");

        // Ensure the combo only progresses after the animation is complete.
        PlayerAnimationHandler.Instance.StopAttackAnimation();
        PlayerAnimationHandler.Instance.PlayAttackAnimation(1, comboStep, this);

        // Temporarily disable player movement during the attack animation
        StartCoroutine(TemporarilyDisableMovement(0.1f));

        // If it's combo step 3 and a finisher is needed, start the finisher
        if (comboStep == 3 && !suppressNormalFinisher)
        {
            StartCoroutine(PerformFinisher());
        }

        // Block input until animation finishes
        suppressInput = true;
        StartCoroutine(EnableInputAfterAnimation());
    }

    private IEnumerator EnableInputAfterAnimation()
    {
        // Wait for the current animation to finish before enabling input
        while (PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0).IsTag("Dagger") && 
               PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Re-enable input after the animation finishes
        suppressInput = false;
    }

    public override void SpawnAttackVFX()
    {
        GameObject vfx = comboStep switch
        {
            1 => slashRedPrefab,
            2 => slashBluePrefab,
            _ => null
        };

        if (vfx != null)
            Instantiate(vfx, attackPoint.position, Quaternion.Euler(90, transform.eulerAngles.y, 0));
    }

    public override void DoHitDetection()
    {
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, 1f, enemyLayers);
        foreach (var enemy in hits)
        {
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                float dmg = attackDamage * PlayerStats.Instance.attackMultiplier;
                target.TakeDamage((int)dmg, enemy.ClosestPoint(attackPoint.position), gameObject);
            }
        }
    }

    private IEnumerator PerformFinisher()
    {
        isFinisherActive = true;

        float dashDistance = 4f;
        float dashDuration = 0.15f;
        float elapsed = 0f;

        Vector3 start = transform.root.position;
        Vector3 dir = transform.root.forward;
        Vector3 end = start + dir * dashDistance;

        // Optional: disable movement during finisher
        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canMove = false;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            transform.root.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // Damage enemies in path + spawn VFX at enemy position
        Collider[] hits = Physics.OverlapCapsule(start, end, 0.6f, enemyLayers);
        foreach (var enemy in hits)
        {
            if (enemy.GetComponentInParent<IDamageable>() is IDamageable target)
            {
                Debug.Log($"[Dagger] Damaging {enemy.name} via parent {target}");
                float dmg = attackDamage * PlayerStats.Instance.attackMultiplier;
                target.TakeDamage((int)dmg, enemy.ClosestPoint(attackPoint.position), gameObject);
            }
            else
            {
                Debug.LogWarning($"[Dagger] {enemy.name} and its parents do NOT have IDamageable");
            }

        }

        yield return new WaitForSeconds(0.1f);

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canMove = true;

        ResetCombo();
        var buffer = FindFirstObjectByType<ModularComboBuffer>();
        buffer?.ClearBuffer();

        // ✅ Also reset all other weapons, just like mix finisher does
        foreach (var w in FindFirstObjectByType<ModularWeaponSlotManager>()?.GetAllWeapons())
            w?.ResetCombo();
    }
public override void SpawnFinisherVFX()
{
    // This is the Eclipse Blades finisher — spawn X-cross slashes at enemies hit during dash
    Collider[] hits = Physics.OverlapCapsule(
        transform.root.position,
        transform.root.position + transform.root.forward * 4f, // dash distance
        0.6f, enemyLayers);

    foreach (var enemy in hits)
    {
        Instantiate(finisherPrefab, enemy.transform.position, Quaternion.identity);
    }

    Debug.Log("[Dagger VFX] Spawned finisher slashes");
}

    public override void HandleMixFinisher(ModularWeaponInput[] combo)
    {
        if (combo.Length < 3 || slotManager == null) return;

        var weapons = slotManager.GetAllWeapons();

        if (combo.Length < 3) return;

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

        if (w1 == null || w2 == null || w3 == null)
            return;

        if (IsCombo<GauntletCombo, GauntletCombo, DaggerCombo>(w1, w2, w3))
        {
            TryBladeBurst();
        }
        else if (IsCombo<SlingShotWeapon, SlingShotWeapon, DaggerCombo>(w1, w2, w3))
        {
            TryTripleBoomerang();
        }
    }

    private bool IsCombo<T1, T2, T3>(ModularWeaponCombo w1, ModularWeaponCombo w2, ModularWeaponCombo w3)
        where T1 : ModularWeaponCombo
        where T2 : ModularWeaponCombo
        where T3 : ModularWeaponCombo
    {
        return w1 is T1 && w2 is T2 && w3 is T3;
    }

    private void TryBladeBurst()
    {
        if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
        PlayerStats.Instance.SpendMana(manaCost);

        BladeBurst.Spawn(transform.root.position, bladeBurstPrefab, 6, 1.2f, 10f);
        ResetCombo();
        suppressNormalFinisher = true;
        FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
    }

    private void TryTripleBoomerang()
    {
        if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
        PlayerStats.Instance.SpendMana(manaCost);

        if (tripleBoomerangPrefab == null) return;

        float spreadAngle = 20f;
        for (int i = -1; i <= 1; i++)
        {
            Quaternion rot = Quaternion.Euler(0, transform.eulerAngles.y + i * spreadAngle, 0);
            Instantiate(tripleBoomerangPrefab, attackPoint.position, rot);
        }

        ResetCombo();
        suppressNormalFinisher = true;
        FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
    }

public override void ResetCombo()
{
    comboStep = 0;
    suppressInput = false;  // Reset suppressInput here to ensure it doesn't carry over.
    suppressNormalFinisher = false;
}

    private IEnumerator TemporarilyDisableMovement(float duration)
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canMove = false;
            yield return new WaitForSeconds(duration);
            PlayerMovement.Instance.canMove = true;
        }
    }
}
