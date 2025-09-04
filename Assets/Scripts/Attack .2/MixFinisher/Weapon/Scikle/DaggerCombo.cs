using UnityEngine;
using System.Collections;

public class DaggerCombo : ModularWeaponCombo
{
    [Header("Basic Combo")]
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    
    [Header("VFX Prefabs")]
    public GameObject slashRedPrefab;
    public GameObject slashBluePrefab;
    public GameObject finisherPrefab;
    public float attackDamage = 12f;

    [Header("Hitbox Settings")]
    public float[] attackRadii = { 1f, 1.2f, 1.4f }; // per step radius

    [Header("Mix Finisher: BladeBurst")]
    public GameObject bladeBurstPrefab;

    [Header("Mix Finisher: TripleBoomerang")]
    public GameObject tripleBoomerangPrefab;
    public int manaCost = 10;

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private bool isFinisherActive = false;
    private float nextAttackAllowedTime = 0f;

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

        // Reset combo if idle for too long
        if (comboStep > 0 && Time.time - lastAttackTime > comboResetDelay)
        {
            Debug.Log("[Dagger] Combo timed out, resetting.");
            ResetCombo();
        }
    }

    public void EnableInputWithDelay(float delay) =>
        StartCoroutine(EnableInputAfterDelay(delay));

    private IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        suppressInput = false;
        Debug.Log("[Dagger] Input re-enabled after equip delay.");
    }

    public override void HandleInput()
    {
        if (suppressInput) return;
        if (Time.time < nextAttackAllowedTime) return; // Prevents instant spam

        var currentState = PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsTag("Dagger"))
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

        // If we just finished Attack3 previously, restart
        if (comboStep >= 3)
            comboStep = 0;

        comboStep++;

        Debug.Log($"[DaggerCombo] Playing Attack Animation -> AttackIndex: {comboStep}");

        PlayerAnimationHandler.Instance.StopAttackAnimation();
        PlayerAnimationHandler.Instance.PlayAttackAnimation(1, comboStep, this); // 1 = Dagger

        StartCoroutine(TemporarilyDisableMovement(0.1f));

        if (comboStep == 3 && !suppressNormalFinisher)
        {
            isFinisherActive = true;
            StartCoroutine(PerformFinisher());
        }

        suppressInput = true;
        StartCoroutine(EnableInputAfterAnimation());
    }

    private IEnumerator EnableInputAfterAnimation()
    {
        var anim = PlayerAnimationHandler.Instance.animator;

        while (anim.GetCurrentAnimatorStateInfo(0).IsTag("Dagger") &&
               anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // If last attack was Attack3, next press starts fresh
        if (comboStep == 3)
            comboStep = 0;

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
        float radius = (comboStep > 0 && comboStep <= attackRadii.Length)
            ? attackRadii[comboStep - 1]
            : 1f;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, radius, enemyLayers);
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
        float dashDistance = 4f;
        float dashDuration = 0.15f;
        float elapsed = 0f;

        Vector3 start = transform.root.position;
        Vector3 dir = transform.root.forward;
        Vector3 end = start + dir * dashDistance;
        
        int borderMask = LayerMask.GetMask("ArenaBorder");
        if (Physics.Raycast(start, dir, out RaycastHit hit, dashDistance, borderMask))
        {
            end = hit.point - dir * 0.2f; // stop slightly before wall
        }

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canMove = false;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            transform.root.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        Collider[] hits = Physics.OverlapCapsule(start, end, 0.6f, enemyLayers);
        foreach (var enemy in hits)
        {
            if (enemy.GetComponentInParent<IDamageable>() is IDamageable target)
            {
                float dmg = attackDamage * PlayerStats.Instance.attackMultiplier;
                target.TakeDamage((int)dmg, enemy.ClosestPoint(attackPoint.position), gameObject);

                if (finisherPrefab != null)
                    Instantiate(finisherPrefab, enemy.transform.position, Quaternion.Euler(90, transform.eulerAngles.y, 0));
            }
        }

        yield return new WaitForSeconds(0.1f);
        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canMove = true;

        ResetCombo();
        FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
        foreach (var w in FindFirstObjectByType<ModularWeaponSlotManager>()?.GetAllWeapons())
            w?.ResetCombo();
    }

    public override void HandleMixFinisher(ModularWeaponInput[] combo)
    {
        if (combo.Length < 3 || slotManager == null) return;

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

        if (IsCombo<GauntletCombo, GauntletCombo, DaggerCombo>(w1, w2, w3))
            TryBladeBurst();
        else if (IsCombo<SlingShotWeapon, SlingShotWeapon, DaggerCombo>(w1, w2, w3))
            TryTripleBoomerang();
    }

    private bool IsCombo<T1, T2, T3>(ModularWeaponCombo w1, ModularWeaponCombo w2, ModularWeaponCombo w3)
        where T1 : ModularWeaponCombo where T2 : ModularWeaponCombo where T3 : ModularWeaponCombo
        => w1 is T1 && w2 is T2 && w3 is T3;

    private void TryBladeBurst()
    {
        if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
        PlayerStats.Instance.SpendMana(manaCost);

        BladeBurst.Spawn(transform.root.position, bladeBurstPrefab, 4, 1.2f, 10f);
        ResetCombo();
        suppressNormalFinisher = true;
        FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
    }

    private void TryTripleBoomerang()
    {
        if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
        PlayerStats.Instance.SpendMana(manaCost);

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
        suppressInput = false;
        suppressNormalFinisher = false;
        isFinisherActive = false;
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
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        // Draw radius for current step if playing in editor
        float radius = (comboStep > 0 && comboStep <= attackRadii.Length)
            ? attackRadii[comboStep - 1]
            : attackRadii[0];

        Gizmos.DrawWireSphere(attackPoint.position, radius);
    }

}
