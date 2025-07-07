using UnityEngine;

public class ExplosionDamageZone : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float radius = 3f;
    public int damage = 30;
    public float delayBeforeDestroy = 0.1f;
    public LayerMask enemyLayers;

    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damage, hit.ClosestPoint(transform.position), gameObject);
            }
        }

        Destroy(gameObject, delayBeforeDestroy); // optional: keep alive briefly for visual clarity
    }
}
