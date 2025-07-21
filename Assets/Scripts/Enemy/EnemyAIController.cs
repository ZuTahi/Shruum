using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    public EnemyData data;
    public EnemySpawner spawner;

    private Transform player;
    private float health;
    private Animator animator;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();

        if (data == null)
        {
            Debug.LogError($"[EnemyAIController] No EnemyData assigned on {gameObject.name}. Destroying self.");
            Destroy(gameObject);
            return;
        }

        health = data.maxHealth;

        switch (data.aiType)
        {
            case EnemyAIType.Aggressive:
                StartCoroutine(AggressiveAI());
                break;
            case EnemyAIType.Ranged:
                StartCoroutine(RangedAI());
                break;
            case EnemyAIType.CollisionRetaliate:
                Debug.LogWarning($"{gameObject.name} AI type CollisionRetaliate not implemented yet.");
                break;
        }
    }

    private IEnumerator AggressiveAI()
    {
        while (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > data.attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                yield return AttackSequence();
            }

            yield return null;
        }
    }

    private IEnumerator RangedAI()
    {
        while (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= data.attackRange)
            {
                yield return AttackSequence();
            }

            yield return null;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * data.moveSpeed * Time.deltaTime;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    private IEnumerator AttackSequence()
    {
        yield return new WaitForSeconds(data.preAttackDuration);

        if (data.weaponPrefab != null)
        {
            GameObject hitbox = Instantiate(data.weaponPrefab, transform.position + transform.forward, transform.rotation);
            EnemyHitbox hb = hitbox.GetComponent<EnemyHitbox>();
            if (hb != null)
            {
                hb.damage = data.attackDamage;
            }
        }

        yield return new WaitForSeconds(data.attackDuration + data.postAttackDuration);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        spawner?.OnEnemyDeath(gameObject);
        Destroy(gameObject);
    }
}
