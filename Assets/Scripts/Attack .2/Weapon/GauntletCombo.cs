using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GauntletCombo : ModularWeaponCombo
{
    public float fixedAttackDelay = 0.3f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public GameObject punchRedPrefab;
    public GameObject punchBluePrefab;
    public GameObject slamAoEPrefab;
    public GameObject bladeSpikePrefab;
    public GameObject vineStrikePrefab;
    public int manaCost = 30;
    public float attackDamage = 15f;
    public float finisherRadius = 1.5f;
    public float finisherMultiplier = 1.5f;

    private int comboStep = 0;
    private float lastAttackTime = 0f;

    private bool isFinisherActive = false;
    private List<Collider> hitEnemies = new();
    protected ModularWeaponSlotManager slotManager;

    protected override void Awake()
    {
        base.Awake();
        slotManager = FindFirstObjectByType<ModularWeaponSlotManager>();
    }

    public override void HandleInput()
    {
        if (suppressNormalFinisher || isFinisherActive || Time.time - lastAttackTime < fixedAttackDelay)
            return;

        ProcessAttack();
    }

    private void ProcessAttack()
    {
        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        PlayerMovement playerMove = GetComponentInParent<PlayerMovement>();
        if (playerMove != null)
            playerMove.TemporarilyLockMovement(0.25f); // Gauntlet might use 0.25s, Slingshot maybe 0.15f

        GameObject vfx = comboStep switch
        {
            1 => punchRedPrefab,
            2 => punchBluePrefab,
            _ => null
        };

        if (vfx != null)
            Instantiate(vfx, attackPoint.position, Quaternion.Euler(90, transform.eulerAngles.y, 0));

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, 0.6f, enemyLayers);
        foreach (var enemy in hits)
        {
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                float finalDamage = attackDamage * PlayerStats.Instance.attackMultiplier;
                target.TakeDamage((int)finalDamage, enemy.ClosestPoint(attackPoint.position), gameObject);
            }
        }

        if (comboStep == 3)
            StartCoroutine(PerformFinisher());
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
        Debug.Log($"{this.name} Performing Finisher");
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


        if (w1 is SlingShotWeapon && w2 is SlingShotWeapon && w3 is GauntletCombo)
        {
            if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            PlayerStats.Instance.SpendMana(manaCost);
            suppressNormalFinisher = true;

            Instantiate(vineStrikePrefab, attackPoint.position, transform.rotation);
            ResetCombo();
        }
        //else if (w1 is SlingShotWeapon && w2 is SlingShotWeapon && w3 is GauntletCombo)
        //{
            //if (!PlayerStats.Instance.HasEnoughMana(manaCost)) return;
            //PlayerStats.Instance.SpendMana(manaCost);

            //Instantiate(bladeSpikePrefab, attackPoint.position, transform.rotation);
            //suppressNormalFinisher = true;
            //ResetCombo();
        //}
    }

    public override void ResetCombo()
    {
        comboStep = 0;
        isFinisherActive = false;
        suppressNormalFinisher = false;
    }
}
