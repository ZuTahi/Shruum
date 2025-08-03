using UnityEngine;
using System.Collections;

public class SlingShotWeapon : ModularWeaponCombo
{
    public float fixedAttackDelay = 0f;
    public float comboResetDelay = 1.5f;
    public Transform attackPoint;
    public GameObject seedProjectilePrefab;
    public GameObject scatterSeedPrefab;

    public float projectileSpeed = 20f;
    public int manaCost = 30;

    [Header("Mix Finisher: OrbitalSickles")]
    public GameObject orbitalSicklePrefab;
    [Header("Mix Finisher: VineStrinke")]
    public GameObject vineStrikePrefab;

    private int comboStep = 0;
    private float lastAttackTime = 0.25f;
    private bool inputBuffered = false;

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
        GameObject proj = Instantiate(seedProjectilePrefab, attackPoint.position, attackPoint.rotation);
        if (proj.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = attackPoint.forward * projectileSpeed;

        StartCoroutine(CheckBufferedInput());
    }

    private void FireScatterShot()
    {
        float[] angles = { -15f, 0f, 15f };
        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, angle, 0) * attackPoint.rotation;
            GameObject proj = Instantiate(scatterSeedPrefab, attackPoint.position, rot);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = rot * Vector3.forward * projectileSpeed;
        }

        ResetCombo();

        var buffer = FindFirstObjectByType<ModularComboBuffer>();
        buffer?.ClearBuffer();

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
            HandleInput();
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
        inputBuffered = false;
        suppressNormalFinisher = false;
    }
}
