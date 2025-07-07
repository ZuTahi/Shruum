using UnityEngine;
using System.Collections;

public class DaggerCombo : ModularWeaponCombo
{
    [Header("Basic Combo")]
    public float fixedAttackDelay = 0.3f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public GameObject slashRedPrefab;
    public GameObject slashBluePrefab;
    public GameObject finisherPrefab;
    public float attackDamage = 12f;

    [Header("Mix Finisher")]
    public GameObject bladeBurstPrefab;
    public GameObject tripleBoomerangPrefab;
    public int manaCost = 10;

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private bool isFinisherActive = false;

    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
    }

    public override void HandleInput()
    {
        if (suppressNormalFinisher || isFinisherActive || Time.time - lastAttackTime < fixedAttackDelay)
        {
            Debug.Log("[Dagger] Input blocked due to suppression or cooldown.");
            return;
        }

        Debug.Log("[Dagger] HandleInput → Performing basic dagger attack");
        ProcessAttack();
    }

    private void ProcessAttack()
    {
        PlayerMovement.Instance.canMove = false;
        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        // 🔒 Freeze player movement briefly
        if (PlayerMovement.Instance != null)
            StartCoroutine(TemporarilyDisableMovement(0.1f));

        GameObject vfx = comboStep switch
        {
            1 => slashRedPrefab,
            2 => slashBluePrefab,
            _ => null
        };

        if (vfx != null)
            Instantiate(vfx, attackPoint.position, Quaternion.Euler(90, transform.eulerAngles.y, 0));

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, 0.6f, enemyLayers);
        foreach (var enemy in hits)
        {
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                float dmg = attackDamage * PlayerStats.Instance.attackMultiplier;
                target.TakeDamage((int)dmg, enemy.ClosestPoint(attackPoint.position), gameObject);
            }
        }

        if (comboStep == 3 && !suppressNormalFinisher)
        {
            StartCoroutine(PerformFinisher());
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
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                float dmg = attackDamage * 1.5f * PlayerStats.Instance.attackMultiplier;
                target.TakeDamage((int)dmg, enemy.ClosestPoint(attackPoint.position), gameObject);

                if (finisherPrefab != null)
                {
                    Vector3 vfxPos = enemy.transform.position;
                    Quaternion vfxRot = Quaternion.LookRotation(transform.forward);
                    Instantiate(finisherPrefab, vfxPos, vfxRot);
                }
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
        suppressNormalFinisher = true;

        BladeBurst.Spawn(transform.root.position, bladeBurstPrefab, 6, 1.2f, 10f);
        ResetCombo();
    }

    private void TryTripleBoomerang()
    {
        if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
        PlayerStats.Instance.SpendMana(manaCost);
        suppressNormalFinisher = true;

        if (tripleBoomerangPrefab == null) return;

        float spreadAngle = 20f;
        for (int i = -1; i <= 1; i++)
        {
            Quaternion rot = Quaternion.Euler(0, transform.eulerAngles.y + i * spreadAngle, 0);
            Instantiate(tripleBoomerangPrefab, attackPoint.position, rot);
        }

        ResetCombo();
    }

    public override void ResetCombo()
    {
        comboStep = 0;
        isFinisherActive = false;
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
