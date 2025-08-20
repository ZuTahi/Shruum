using UnityEngine;

public static class BladeBurst
{
    public static void Spawn(Vector3 origin, GameObject projectilePrefab, int count, float radius, float speed)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("BladeBurst: projectile prefab not assigned!");
            return;
        }

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad + 45*Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 spawnPos = origin + direction * radius;

            GameObject blade = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

            if (blade.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = direction * speed;
        }
    }
}
