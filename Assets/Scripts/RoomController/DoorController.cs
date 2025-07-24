using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Components")]
    public Animator doorAnimator; // Animator with isLocked bool controlling open/close

    /// <summary>
    /// Lock the door: activate collider and play lock animation.
    /// </summary>

    /// <summary>
    /// Unlock the door: play open animation. Collider stays enabled if it's animated.
    /// </summary>
    public void Unlock()
    {
        // We no longer disable the collider, since the animated mesh moves away
        if (doorAnimator != null)
            doorAnimator.SetBool("isLocked", false);

        Debug.Log($"{gameObject.name} is now Unlocked.");
    }
}
