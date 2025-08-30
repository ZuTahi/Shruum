using UnityEngine;

public class ExplosionDamageZone : MonoBehaviour
{
    public int damage = 15;  // Amount of damage to apply
    public float radius = 5f; // Radius of the explosion
    public float effectDuration = 1f;  // Duration for the explosion effect to last
    private float effectTimer = 0f; // Timer to destroy the explosion effect after duration

    private Renderer rend;
    private Color originalColor;

    private void Start()
    {
        // Get the renderer and set up the explosion effect (if it's part of the same prefab)
        rend = GetComponent<Renderer>();
        if (rend != null) originalColor = rend.material.color;

        // Optionally, if it's a particle system within this object, you can control that here.
        // For example, we could destroy this object after some time to clean up the explosion effect.
        Destroy(gameObject, effectDuration);  // Destroy after the effectDuration to remove explosion after it ends
    }

    // Triggered when an object enters the explosion damage zone
    private void OnTriggerEnter(Collider other)
    {
        // Apply damage to any object that enters the explosion radius (in damageable layer)
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            IDamageable damageableObject = other.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                damageableObject.TakeDamage(damage, transform.position, gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the explosion radius in the editor (gizmo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void Update()
    {
        // If you want to add some visual effect changes over time, you can handle it here
        if (rend != null)
        {
            // Example: Change color or effect timing
            effectTimer += Time.deltaTime;
            if (effectTimer >= effectDuration)
            {
                // Optionally, change the material back to the original color after the effect is done
                rend.material.color = originalColor;
            }
        }
    }
}
