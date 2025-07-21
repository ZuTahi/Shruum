using UnityEngine;

public class ExplosiveSeed : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float travelSpeed = 12f;
    public float maxLifetime = 3f;
    public int directDamage = 10;

    public GameObject explosionEffectPrefab; // Particle system prefab

    private float timer = 0f;

    void Update()
    {
        transform.position += transform.forward * travelSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        if (timer > maxLifetime)
        {
            Explode(null); // No direct hit
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SeedProjectile"))
            return;

        // Deal direct hit damage if it's an enemy
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            int finalDamage = Mathf.CeilToInt(directDamage * PlayerStats.Instance.attackMultiplier);
            target.TakeDamage(finalDamage, other.ClosestPoint(transform.position), gameObject);
            Explode(other.gameObject);
        }
        else
        {
            Explode(other.gameObject);
        }
    }

    private void Explode(GameObject hitObject)
    {
        if (explosionEffectPrefab)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
