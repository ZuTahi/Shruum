using UnityEngine;

public class ExplosiveSeed : MonoBehaviour
{
    public GameObject explosionDamageZonePrefab; // ExplosionDamageZone prefab
    public float explosionRadius = 5f;   // Explosion radius for collision
    public float explosionForce = 10f;    // Explosion force (optional)
    public LayerMask damageableLayer;     // Layer to damage (e.g., enemies, player)
    public float lifetime = 5f;           // Lifetime of the seed before it explodes (optional)

    private void OnCollisionEnter(Collision collision)
    {
        // Instantiate ExplosionDamageZone at the point of collision
        Instantiate(explosionDamageZonePrefab, transform.position, Quaternion.identity);

        // Destroy the ExplosiveSeed after it collides (and spawns the ExplosionDamageZone)
        Destroy(gameObject);
    }

    private void Start()
    {
        // Optionally destroy after lifetime to avoid it staying in the world forever
        Destroy(gameObject, lifetime);
    }
}
