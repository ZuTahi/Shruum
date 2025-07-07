using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VineStrike : MonoBehaviour
{
    public float maxLength = 6f;
    public float growSpeed = 12f;
    public int damage = 25;
    public LayerMask enemyLayers;
    public float width = 0.6f;

    private float currentLength = 0f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x, originalScale.y, 0);
    }

    void Update()
    {
        if (currentLength >= maxLength) return;

        float growAmount = growSpeed * Time.deltaTime;
        currentLength += growAmount;
        currentLength = Mathf.Min(currentLength, maxLength);

        transform.localScale = new Vector3(originalScale.x, originalScale.y, currentLength);

        // Damage enemies along the grown path
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
