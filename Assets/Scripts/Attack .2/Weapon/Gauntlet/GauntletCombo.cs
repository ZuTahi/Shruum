using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GauntletCombo : ModularWeaponCombo
{
    [Header("Basic Combo")]
    public float fixedAttackDelay = 0.3f;
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public GameObject punchRedPrefab;
    public GameObject punchBluePrefab;
    public GameObject slamAoEPrefab;
    public float attackDamage = 15f;

    [Header("Finisher")]
    public float finisherRadius = 1.5f;
    public float finisherMultiplier = 1.5f;

    [Header("Mix Finishers")]
    public int manaCost = 30;
    public GameObject bladeSpikePrefab;
    public GameObject explosiveSeedPrefab;
    public float projectileSpeed = 20f;

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private bool isFinisherActive = false;

    private List<Collider> hitEnemies = new();
    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
        suppressInput = true;
    }

    void Update()
    {
        if (suppressInput) return;
        if (comboStep > 0 && Time.time - lastAttackTime > comboResetDelay)
        {
            Debug.Log("[Gauntlet] Combo timed out, resetting.");
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
        Debug.Log("[Gauntlet] Input re-enabled after equip delay.");
    }

    public override void HandleInput()
    {
        if (suppressInput) return;

        // Block if animation still playing
        var currentState = PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsTag("Gauntlet") && currentState.normalizedTime < 1f)
        {
            Debug.Log("[GauntletCombo] Animation still playing, blocking input.");
            return;
        }

        ProcessAttack();
    }

    private void ProcessAttack()
    {
        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        Debug.Log($"[GauntletCombo] Playing Attack Animation -> AttackIndex: {comboStep}");

        // Stop any ongoing attack anim, then play the new one
        PlayerAnimationHandler.Instance.StopAttackAnimation();
        PlayerAnimationHandler.Instance.PlayAttackAnimation(2, comboStep, this); // 2 = Gauntlet

        // Temporarily lock movement
        StartCoroutine(TemporarilyDisableMovement(0.1f));

        // Finisher trigger
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
        while (PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0).IsTag("Gauntlet") &&
               PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
        suppressInput = false;
    }

    // ✅ Only triggered via animation event
    public override void SpawnAttackVFX()
    {
        Debug.Log($"[VFX SPAWN] Weapon: {weaponType}, ComboStep: {comboStep}, Time: {Time.time}, Caller: {new System.Diagnostics.StackTrace()}");

        GameObject vfx = comboStep switch
        {
        1 => punchRedPrefab,
        2 => punchBluePrefab,
        _ => null
        };

        if (vfx != null)
        Instantiate(vfx, attackPoint.position, Quaternion.Euler(90, transform.eulerAngles.y, 0));
    }

    public override void DoHitDetection()
    {
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, 0.6f, enemyLayers);
        foreach (var enemy in hits)
        {
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                float finalDamage = attackDamage * PlayerStats.Instance.attackMultiplier;
                target.TakeDamage((int)finalDamage, enemy.ClosestPoint(attackPoint.position), gameObject);
            }
        }
    }

    IEnumerator PerformFinisher()
    {
        isFinisherActive = true;
        hitEnemies.Clear();

        Vector3 slamPos = transform.root.position;

        if (slamAoEPrefab != null)
            Instantiate(slamAoEPrefab, slamPos, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(slamPos, finisherRadius, enemyLayers);
        foreach (var enemy in hits)
        {
            if (!hitEnemies.Contains(enemy))
            {
                hitEnemies.Add(enemy);
                if (enemy.TryGetComponent<IDamageable>(out var target))
                {
                    float dmg = attackDamage * finisherMultiplier * PlayerStats.Instance.attackMultiplier;
                    target.TakeDamage((int)dmg, enemy.ClosestPoint(slamPos), gameObject);
                }
            }
        }

        yield return new WaitForSeconds(0.3f);
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

        // Blade Spike
        if (w1 is DaggerCombo && w2 is DaggerCombo && w3 is GauntletCombo)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);

            var spike = Instantiate(bladeSpikePrefab, transform.root.position, Quaternion.identity);
            if (spike.TryGetComponent<BladeSpike>(out var spikeScript))
                spikeScript.followTarget = transform.root;

            ResetCombo();
            suppressNormalFinisher = true;
            FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
        }
        // Explosive Seed
        else if (w1 is SlingShotWeapon && w2 is SlingShotWeapon && w3 is GauntletCombo)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);

            GameObject proj = Instantiate(explosiveSeedPrefab, attackPoint.position, attackPoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = attackPoint.forward * projectileSpeed;

            ResetCombo();
            suppressNormalFinisher = true;
            FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
        }
    }

    public override void ResetCombo()
    {
        comboStep = 0;
        suppressInput = false;
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
