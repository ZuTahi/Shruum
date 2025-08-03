using UnityEngine;

public abstract class ModularWeaponCombo : MonoBehaviour
{
    public bool suppressInput = false;  // ✅ Added this
    public bool suppressNormalFinisher = false;
    public WeaponType weaponType;

    public abstract void HandleInput();

    public virtual void HandleMixFinisher(ModularWeaponInput[] combo) { }
    public virtual void ResetCombo() { }
    protected virtual void Awake() { }

}
