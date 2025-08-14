using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    public static PlayerAnimationHandler Instance;

    [Header("Animator")]
    public Animator animator;

    [Header("Weapon Meshes")]
    public GameObject[] daggerMeshes;   // Can have multiple dagger parts
    public GameObject[] gauntletMeshes; // Can have multiple gauntlet parts
    public GameObject slingshotMesh;    // Single mesh

    [HideInInspector] public ModularWeaponCombo currentWeapon;
    private WeaponType currentWeaponType = WeaponType.None;

    // Cached Animator parameter hashes (faster than string lookups)
    private static readonly int HashWeaponType        = Animator.StringToHash("WeaponType");
    private static readonly int HashAttackIndex       = Animator.StringToHash("AttackIndex");
    private static readonly int HashMixFinisherIndex  = Animator.StringToHash("MixFinisherIndex");
    private static readonly int HashIsAttacking       = Animator.StringToHash("IsAttacking");

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Called by attack scripts to start animation
    /// </summary>
    public void PlayAttackAnimation(int weaponType, int attackIndex, ModularWeaponCombo weapon, int mixFinisherIndex = 0)
    {
        currentWeapon = weapon;
        currentWeaponType = (WeaponType)weaponType;

        animator.SetInteger(HashWeaponType, weaponType);
        animator.SetInteger(HashAttackIndex, attackIndex);
        animator.SetInteger(HashMixFinisherIndex, mixFinisherIndex); // harmless if parameter not in Animator
        animator.SetBool(HashIsAttacking, true);
    }

    public void StopAttackAnimation()
    {
        animator.SetBool(HashIsAttacking, false);
        // Reset combo when we fully stop attacking
    }

    /// <summary>
    /// Animation Event — exact frame of impact
    /// </summary>
    public void OnAttackHit()
    {
        currentWeapon?.SpawnAttackVFX();
        currentWeapon?.DoHitDetection();
    }

    /// <summary>
    /// Animation Event — finisher visual effect only
    /// </summary>
public void OnFinisherEvent()
{
    currentWeapon?.ExecuteFinisher();
}


    /// <summary>
    /// Animation Event — show selected weapon and hide others
    /// </summary>
    public void ShowWeapon(int weaponTypeInt)
    {
        WeaponType type = (WeaponType)weaponTypeInt;
        currentWeaponType = type;

        switch (type)
        {
            case WeaponType.Dagger:
                SetMeshActive(daggerMeshes, true);
                SetMeshActive(gauntletMeshes, false);
                if (slingshotMesh) slingshotMesh.SetActive(false);
                break;

            case WeaponType.Gauntlet:
                SetMeshActive(daggerMeshes, false);
                SetMeshActive(gauntletMeshes, true);
                if (slingshotMesh) slingshotMesh.SetActive(false);
                break;

            case WeaponType.Slingshot:
                SetMeshActive(daggerMeshes, false);
                SetMeshActive(gauntletMeshes, false);
                if (slingshotMesh) slingshotMesh.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Hide all weapons — called at spawn or on death
    /// </summary>
    public void HideAllWeapons()
    {
        SetMeshActive(daggerMeshes, false);
        SetMeshActive(gauntletMeshes, false);
        if (slingshotMesh) slingshotMesh.SetActive(false);

        currentWeaponType = WeaponType.None;
    }

    /// <summary>
    /// Helper to set multiple meshes active/inactive
    /// </summary>
    private void SetMeshActive(GameObject[] meshes, bool active)
    {
        if (meshes == null) return;
        foreach (var m in meshes)
            if (m) m.SetActive(active);
    }
}
