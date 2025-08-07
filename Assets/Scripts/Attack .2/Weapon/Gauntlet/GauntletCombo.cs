using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GauntletCombo : ModularWeaponCombo
{
    public float fixedAttackDelay = 0.3f;
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public GameObject punchRedPrefab;
    public GameObject punchBluePrefab;
    public GameObject slamAoEPrefab;
    public int manaCost = 30;
    public float attackDamage = 15f;

    public float finisherRadius = 1.5f;
    public float finisherMultiplier = 1.5f;

    [Header("Mix Finisher: BladeSpike")]
    public GameObject bladeSpikePrefab;
    [Header("Mix Finisher: ExplosiveSeed")]
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
        if (suppressNormalFinisher || isFinisherActive || Time.time - lastAttackTime < fixedAttackDelay)
            return;

        ProcessAttack();
    }

    private void ProcessAttack()
    {
        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        PlayerAnimationHandler.Instance.StopAttackAnimation();
        PlayerAnimationHandler.Instance.PlayAttackAnimation(2, comboStep, this);

        // Lock movement briefly
        if (PlayerMovement.Instance != null)
            StartCoroutine(TemporarilyDisableMovement(0.1f));

        // Tell animator to play attack animation
        PlayerAnimationHandler.Instance.PlayAttackAnimation(2, comboStep, this); // 2 = Gauntlet

        // Finisher check stays here so combo logic is untouched
        if (comboStep == 3)
            StartCoroutine(PerformFinisher());
    }

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
    public override void DoHitDetection()
    {
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, 0.6f, enemyLayers);
        foreach (var enemy in hits)
        {
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                float finalDamage = attackDamage * PlayerData.attackMultiplier;
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
                    float dmg = attackDamage * finisherMultiplier * PlayerData.attackMultiplier;
                    target.TakeDamage((int)dmg, enemy.ClosestPoint(slamPos), gameObject);
                }
            }
        }

        yield return new WaitForSeconds(0.3f);
        ResetCombo();
        Debug.Log($"{this.name} Performing Finisher");
        var buffer = FindFirstObjectByType<ModularComboBuffer>();
        buffer?.ClearBuffer();

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
        isFinisherActive = false;
        suppressNormalFinisher = false;
    }
    private IEnumerator TemporarilyDisableMovement(float duration)
    {
        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
        {
            movement.canMove = false;
            yield return new WaitForSeconds(duration);
            movement.canMove = true;
        }
    }
}
