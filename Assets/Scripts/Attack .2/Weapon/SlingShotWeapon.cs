using UnityEngine;
using System.Collections;

public class SlingShotWeapon : ModularWeaponCombo
{
    public float fixedAttackDelay = 0f;
    public Transform shootPoint;
    public GameObject seedProjectilePrefab;
    public GameObject scatterSeedPrefab;
    public GameObject boomerangSeedPrefab;
    public GameObject explosiveSeedPrefab;
    public float projectileSpeed = 20f;
    public int manaCost = 30;

    private int comboStep = 0;
    private float lastAttackTime = 0.25f;
    private bool inputBuffered = false;

    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
    }

    public override void HandleInput()
    {
        if (!gameObject.activeInHierarchy)
            return;
        suppressNormalFinisher = false;

        if (Time.time - lastAttackTime < fixedAttackDelay)
        {
            if (!inputBuffered)
            {
                Debug.Log("[InputBuffer] Queued input during cooldown");
                inputBuffered = true;
            }
            return;
        }

        PerformAttackStep();
    }

    private void PerformAttackStep()
    {
        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        Debug.Log($"[PerformAttackStep] Combo Step: {comboStep}");

        if (comboStep == 3)
            FireScatterShot();
        else
            FireSingleSeed();
    }

    private void FireSingleSeed()
    {
        GameObject proj = Instantiate(seedProjectilePrefab, shootPoint.position, shootPoint.rotation);
        if (proj.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = shootPoint.forward * projectileSpeed;

        StartCoroutine(CheckBufferedInput());
    }

    private void FireScatterShot()
    {
        float[] angles = { -15f, 0f, 15f };
        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, angle, 0) * shootPoint.rotation;
            GameObject proj = Instantiate(scatterSeedPrefab, shootPoint.position, rot);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = rot * Vector3.forward * projectileSpeed;
        }

        ResetCombo();

        // ✅ Clear combo buffer
        var buffer = FindFirstObjectByType<ModularComboBuffer>();
        buffer?.ClearBuffer();

        // ✅ Reset all other weapons to avoid lingering combo state
        var slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
        foreach (var w in slotManager?.GetAllWeapons())
            w?.ResetCombo();

        StartCoroutine(CheckBufferedInput());
    }

    private IEnumerator CheckBufferedInput()
    {
        yield return new WaitForSeconds(fixedAttackDelay * 0.5f);

        if (inputBuffered)
        {
            Debug.Log("[InputBuffer] Processing buffered input");
            inputBuffered = false;
            HandleInput(); // safely triggers next combo step
        }
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

        if (w1 == null || w2 == null || w3 == null)
            return;

        // LLJ → Boomerang Seeds
        if (w1 is SlingShotWeapon && w2 is SlingShotWeapon && w3 is DaggerCombo)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);

            for (int i = 0; i < 3; i++)
            {
                float angle = -10 + i * 10;
                Quaternion rot = Quaternion.Euler(0, angle, 0) * shootPoint.rotation;
                Instantiate(boomerangSeedPrefab, shootPoint.position, rot);
            }

            suppressNormalFinisher = true;
            ResetCombo();
        }
        // LLK → Explosive Seed
        else if (w1 is SlingShotWeapon && w2 is SlingShotWeapon && w3 is GauntletCombo)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);

            GameObject proj = Instantiate(explosiveSeedPrefab, shootPoint.position, shootPoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = shootPoint.forward * projectileSpeed;

            suppressNormalFinisher = true;
            ResetCombo();
        }
    }

    public override void ResetCombo()
    {
        comboStep = 0;
        inputBuffered = false;
        suppressNormalFinisher = false;
    }
}
