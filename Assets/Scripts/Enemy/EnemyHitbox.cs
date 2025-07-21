using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 0.2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}