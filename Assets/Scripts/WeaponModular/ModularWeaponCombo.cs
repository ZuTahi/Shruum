using UnityEngine;

/// <summary>
/// Base class for all modular weapons.
/// Each weapon overrides HandleInput and optionally supports mix finishers.
/// </summary>
public abstract class ModularWeaponCombo : MonoBehaviour
{
    /// <summary>
    /// Flag used to suppress the default finisher if a mix finisher was triggered.
    /// </summary>
    public bool suppressNormalFinisher = false;
    public WeaponType weaponType;

    /// <summary>
    /// Called when the player presses this weapon's input key (J/K/L).
    /// </summary>
    public abstract void HandleInput();

    /// <summary>
    /// Called when this weapon is the third input in a valid mix finisher combo.
    /// Override in derived classes to implement finisher behavior.
    /// </summary>
    public virtual void HandleMixFinisher(ModularWeaponInput[] combo) { }

    /// <summary>
    /// Resets internal combo state for the weapon. Optional to override.
    /// </summary>
    public virtual void ResetCombo() { }

    /// <summary>
    /// Allows derived weapons to hook into Awake() without breaking base logic.
    /// </summary>
    protected virtual void Awake() { }
}
