using UnityEngine;

public class BladeSpike : MonoBehaviour
{
    public Transform followTarget;
    public float damageInterval = 0.5f;
    public float duration = 3f;
    public float damageRadius = 1.2f;
    public int damage = 15;
    public LayerMask enemyLayers;

    private float timer = 0f;
    private float damageTimer = 0f;

    void Start()
    {
        timer = duration;
        damageTimer = damageInterval;
    }

    void Update()
    {
        if (followTarget != null)
            transform.position = followTarget.position;

        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0f)
        {
            DealDamage();
            damageTimer = damageInterval;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
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
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f); // red-ish transparent
        Gizmos.DrawSphere(transform.position, damageRadius);
    }
}
