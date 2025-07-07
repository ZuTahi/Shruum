using UnityEngine;

public class BladeProjectile : MonoBehaviour
{
    public int damage = 20;
    public float lifetime = 1.5f;
    public float damageRadius = 0.5f;
    public LayerMask enemyLayers;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, enemyLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage, hit.ClosestPoint(transform.position), gameObject);
            }
        }
    }
}
