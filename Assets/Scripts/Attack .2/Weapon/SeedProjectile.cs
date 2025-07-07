using UnityEngine;

public class SeedProjectile : MonoBehaviour
{
    public int baseDamage = 10;
    public float lifetime = 3f;
    private GameObject shooter;

    public void SetShooter(GameObject origin)
    {
        shooter = origin;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignore shooter and other projectiles
        if (other.gameObject == shooter || other.CompareTag("SeedProjectile"))
            return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            int finalDamage = Mathf.CeilToInt(baseDamage * PlayerStats.Instance.attackMultiplier);
            target.TakeDamage(baseDamage, transform.position, gameObject);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}