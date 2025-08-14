using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GauntletCombo : ModularWeaponCombo
{
    [Header("Basic Settings")]
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float attackDamage = 15f;

    [Header("VFX Prefabs")]
    public GameObject punchRedPrefab;
    public GameObject punchBluePrefab;
    public GameObject slamAoEPrefab;

    [Header("Finisher Settings")]
    public float finisherRadius = 1.5f;
    public float finisherMultiplier = 1.5f;

    [Header("Mix Finishers")]
    public int manaCost = 30;
    public GameObject bladeSpikePrefab;
    public GameObject explosiveSeedPrefab;
    public float projectileSpeed = 20f;

    [Header("Spam protection")]
    [Tooltip("Minimum seconds between accepting attacks (helps prevent instant spam advancing).")]
    public float minAttackGap = 0.12f;

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private bool isFinisherActive = false;
    private float nextAttackAllowedTime = 0f;
    private List<Collider> hitEnemies = new();
    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
        suppressInput = true; // won't accept input until enabled (equip)
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

    public void EnableInputWithDelay(float delay) =>
        StartCoroutine(EnableInputAfterDelay(delay));

    private IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        suppressInput = false;
        Debug.Log("[Gauntlet] Input re-enabled after equip delay.");
    }

public override void HandleInput()
{
    if (suppressInput) return;

    // ✅ Stop here if still cooling down — don't even call ProcessAttack()
    if (Time.time < nextAttackAllowedTime) 
        return;

    var currentState = PlayerAnimationHandler.Instance.animator.GetCurrentAnimatorStateInfo(0);

    if (currentState.IsTag("Gauntlet"))
    {
        if (PlayerAnimationHandler.Instance.animator.IsInTransition(0) ||
            currentState.normalizedTime < 0.85f)
            return;
    }

    // ✅ Only called if no cooldown and anim ready
    ProcessAttack();
}

private void ProcessAttack()
{
    lastAttackTime = Time.time;

    if (comboStep >= 3)
        comboStep = 0;

    comboStep++;

    Debug.Log($"[GauntletCombo] Playing Attack Animation -> AttackIndex: {comboStep}");

    PlayerAnimationHandler.Instance.StopAttackAnimation();
    PlayerAnimationHandler.Instance.PlayAttackAnimation(2, comboStep, this);

    // ✅ Only disable movement if attack actually triggers
    StartCoroutine(TemporarilyDisableMovement(0.1f));

    if (comboStep == 3 && !suppressNormalFinisher)
        isFinisherActive = true;

    suppressInput = true;

    // ✅ Set cooldown AFTER movement disable — ensures next spam won’t re-trigger it
    nextAttackAllowedTime = Time.time + minAttackGap;

    StartCoroutine(EnableInputAfterAnimation());
}

    private IEnumerator EnableInputAfterAnimation()
    {
        var anim = PlayerAnimationHandler.Instance.animator;

        // wait until the current gauntlet attack animation is finished
        while (anim.GetCurrentAnimatorStateInfo(0).IsTag("Gauntlet") &&
               anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // After animation ends, if the last attack was Attack3 reset comboStep
        if (comboStep == 3)
            comboStep = 0;

        suppressInput = false;
    }

    // Called from animation event for normal punches
    public override void SpawnAttackVFX()
    {
        GameObject vfx = comboStep switch
        {
            1 => punchRedPrefab,
            2 => punchBluePrefab,
            _ => null
        };

        if (vfx != null)
            Instantiate(vfx, attackPoint.position, Quaternion.Euler(90, transform.eulerAngles.y, 0));
    }

    // Called from animation event for normal punches
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

    // Called from Animation Event for Finisher Impact (must be called by animation)
    public override  void ExecuteFinisher()
    {
        if (!isFinisherActive) return;

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

        // Finisher complete — reset everything
        FindFirstObjectByType<ModularComboBuffer>()?.ClearBuffer();
        foreach (var w in FindFirstObjectByType<ModularWeaponSlotManager>()?.GetAllWeapons())
            w?.ResetCombo();

        isFinisherActive = false;
        ResetCombo();
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
        isFinisherActive = false;
        hitEnemies.Clear();

        if (PlayerAnimationHandler.Instance != null)
            PlayerAnimationHandler.Instance.animator.SetInteger("AttackIndex", 0);
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
