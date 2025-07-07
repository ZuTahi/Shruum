using UnityEngine;

public class BoomerangSeed : MonoBehaviour
{
    [Header("Boomerang Settings")]
    public float speed = 12f;
    public float returnSpeed = 14f;
    public float travelTime = 0.7f;
    public float extraLifetime = 1.2f;
    public float damageRadius = 0.5f;
    public int damage = 10;

    [Header("Detection & Layers")]
    public LayerMask enemyLayers;
    public LayerMask obstacleLayers; // ← assign this in the Inspector

    private enum Phase { Outbound, Returning, Exiting }
    private Phase phase = Phase.Outbound;

    private float timer = 0f;
    private Vector3 spawnPosition;
    private Vector3 travelDirection;
    private Vector3 returnDirection;
    private float exitTimer = 0f;

    private void Start()
    {
        spawnPosition = transform.position;
        travelDirection = transform.forward;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Outbound:
                transform.position += travelDirection * speed * Time.deltaTime;
                if (timer >= travelTime)
                {
                    phase = Phase.Returning;
                    returnDirection = (spawnPosition - transform.position).normalized;
                }
                break;

            case Phase.Returning:
                transform.position += returnDirection * returnSpeed * Time.deltaTime;
                if (Vector3.Distance(transform.position, spawnPosition) < 0.5f)
                {
                    phase = Phase.Exiting;
                    exitTimer = 0f;
                }
                break;

            case Phase.Exiting:
                transform.position += returnDirection * returnSpeed * Time.deltaTime;
                exitTimer += Time.deltaTime;
                if (exitTimer > extraLifetime)
                    Destroy(gameObject);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore self, other seeds, player
        if (other.CompareTag("SeedProjectile") || other.CompareTag("Player"))
            return;

        // Damage enemies but keep flying
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage, transform.position, gameObject);
            return; // do NOT destroy
        }

        // Check if it hit an obstacle layer (e.g. wall, tree, rock)
        if (((1 << other.gameObject.layer) & obstacleLayers) != 0)
        {
            Destroy(gameObject);
        }
    }
}
