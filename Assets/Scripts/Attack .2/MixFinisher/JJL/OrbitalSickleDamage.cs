using UnityEngine;

public class OrbitalSickleDamage : MonoBehaviour
{
    public int damage = 15;
    public float damageRadius = 0.5f;
    public LayerMask enemyLayers;
    public float damageCooldown = 0.3f;

    private float lastDamageTime = 0f;

    void Update()
    {
        if (Time.time - lastDamageTime >= damageCooldown)
        {
            DealDamage();
            lastDamageTime = Time.time;
        }
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
        Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.4f); // green-ish transparent
        Gizmos.DrawSphere(transform.position, damageRadius);
    }
}
