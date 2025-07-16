using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Components")]
    public Collider doorCollider; // The collider that blocks the player
    public Animator doorAnimator; // Optional: Animator for door open/close animations

    /// <summary>
    /// Lock the door: activate collider and play lock animation.
    /// </summary>
    public void Lock()
    {
        if (doorCollider != null)
            doorCollider.enabled = true;

        if (doorAnimator != null)
            doorAnimator.SetBool("isLocked", true);

        Debug.Log($"{gameObject.name} is now Locked.");
    }

    /// <summary>
    /// Unlock the door: disable collider and play unlock animation.
    /// </summary>
    public void Unlock()
    {
        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorAnimator != null)
            doorAnimator.SetBool("isLocked", false);

        Debug.Log($"{gameObject.name} is now Unlocked.");
    }
}
