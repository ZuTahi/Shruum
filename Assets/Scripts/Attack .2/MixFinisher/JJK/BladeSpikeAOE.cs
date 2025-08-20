using UnityEngine;

public class BladeSpike : MonoBehaviour
{
    public float lifeTime = 1.5f;       // lasts for 1.5s
    public float damageRadius = 1.2f;
    public int damage = 15;
    public LayerMask enemyLayers;

    void Start()
    {
        DealDamage();                   // deal damage once when spawned
        Destroy(gameObject, lifeTime);  // despawn after 1.5s
    }

    void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, enemyLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                int finalDamage = Mathf.CeilToInt(damage * PlayerStats.Instance.attackMultiplier);
                target.TakeDamage(finalDamage, hit.ClosestPoint(transform.position), gameObject);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawSphere(transform.position, damageRadius);
    }
}
