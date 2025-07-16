using UnityEngine;
using System.Collections;

public class VineStrike : MonoBehaviour
{
    public float maxLength = 6f;
    public float growSpeed = 12f;
    public int damage = 25;
    public LayerMask enemyLayers;
    public float width = 0.6f;

    public float destroyDelay = 1.5f; // seconds after full length
    private bool hasFullyGrown = false;
    private float destroyTimer = 0f;

    public float damageInterval = 0.5f;
    private float damageTimer = 0f;

    private float currentLength = 0f;
    private Vector3 originalScale;

    private Transform visual; // Reference to the child mesh

    void Start()
    {
        visual = transform.GetChild(0); // assumes first child is the mesh
        originalScale = visual.localScale;
        visual.localScale = new Vector3(originalScale.x, originalScale.y, 0.01f);
        visual.localPosition = Vector3.zero;
    }

    void Update()
    {
        if (!hasFullyGrown)
        {
            float growAmount = growSpeed * Time.deltaTime;
            currentLength += growAmount;
            currentLength = Mathf.Min(currentLength, maxLength);

            // Update visual mesh: scale forward only
            visual.localScale = new Vector3(originalScale.x, originalScale.y, currentLength);
            visual.localPosition = new Vector3(0, 0, currentLength / 2f);

            // ✅ Only deal damage at interval
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                DealDamage();
                damageTimer = damageInterval;
            }

            if (currentLength >= maxLength)
            {
                hasFullyGrown = true;
                destroyTimer = destroyDelay;
            }
        }
        else
        {
            // ✅ Keep dealing damage even after fully grown
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                DealDamage();
                damageTimer = damageInterval;
            }

            destroyTimer -= Time.deltaTime;
            if (destroyTimer <= 0f)
                Destroy(gameObject);
        }
    }
    void DealDamage()
    {
        Vector3 center = transform.position + transform.forward * (currentLength / 2f);
        Vector3 halfExtents = new Vector3(width / 2f, 1f, currentLength / 2f);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, enemyLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage, hit.ClosestPoint(center), gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + transform.forward * (currentLength / 2f);
        Vector3 halfExtents = new Vector3(width / 2f, 1f, currentLength / 2f);
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(center - transform.position, halfExtents * 2f);
    }
}
