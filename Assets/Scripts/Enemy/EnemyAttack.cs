using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public GameObject hitboxPrefab;
    public Transform attackOrigin;
    public float hitboxOffset = 1.2f;

    public float windUpTime = 0.3f;
    public float attackRecoveryTime = 0.4f;

    private EnemyStats stats;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    public void SpawnHitbox()
    {
        if (hitboxPrefab == null)
        {
            Debug.LogWarning("Hitbox prefab missing!");
            return;
        }

        Vector3 spawnPos = attackOrigin != null
            ? attackOrigin.position
            : transform.position + transform.forward * hitboxOffset;

        GameObject go = Instantiate(hitboxPrefab, spawnPos, transform.rotation);
        EnemyHitbox hb = go.GetComponent<EnemyHitbox>();
        if (hb != null)
            hb.damage = stats.attackDamage;

        Debug.Log("Spawned hitbox at " + spawnPos);
    }
}
