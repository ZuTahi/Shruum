using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour, IDamageable
{
    public EnemyData data;
    public EnemySpawner spawner;

    private Transform player;
    private float health;
    private Animator animator;

    // ✅ Added fields
    private bool isStaggered = false;
    private float staggerDuration = 0.2f;

    private Renderer rend;
    private Color originalColor;
    private NavMeshAgent agent;

    [Header("Drop Settings")]
    public EnemyDrop enemyDropPrefab;
    public int minNatureForceDrop = 1;
    public int maxNatureForceDrop = 3;
    public int minManaDrop = 0;
    public int maxManaDrop = 2;
    [Range(0f, 1f)] public float loreNoteDropChance = 0.15f;

    private void Start()
    {
        if (data.usesNavMesh)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = data.moveSpeed;
            }
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

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
            if (isStaggered)
            {
                yield return null;
                continue;
            }

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
            if (isStaggered)
            {
                yield return null;
                continue;
            }

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

        if (data.usesNavMesh && agent != null)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // Direct move for ghosts
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * data.moveSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
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

    // ===== IDamageable Implementation =====
    public void TakeDamage(int amount, Vector3 hitPoint, GameObject source)
    {
        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage from {source.name}");

        StartCoroutine(OnHitReaction());

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator OnHitReaction()
    {
        isStaggered = true;

        StartCoroutine(FlickerRed());

        yield return new WaitForSeconds(staggerDuration);

        isStaggered = false;
    }

    private IEnumerator FlickerRed()
    {
        if (rend == null) yield break;

        Color red = Color.red;
        rend.material.color = red;
        yield return new WaitForSeconds(0.05f);

        rend.material.color = originalColor;
        yield return new WaitForSeconds(0.05f);

        rend.material.color = red;
        yield return new WaitForSeconds(0.05f);

        rend.material.color = originalColor;
    }

    private void Die()
    {
        DropRewards();
        spawner?.OnEnemyDeath(gameObject);
        Destroy(gameObject);
    }
    private void DropRewards()
    {
        if (enemyDropPrefab != null)
        {
            var drop = Instantiate(enemyDropPrefab, transform.position, Quaternion.identity);
            int natureForce = Random.Range(minNatureForceDrop, maxNatureForceDrop + 1);
            int mana = Random.Range(minManaDrop, maxManaDrop + 1);

            drop.SetDropValues(mana, natureForce);
            drop.DropItems();
        }

        if (Random.value < loreNoteDropChance)
        {
            PlayerInventory.Instance?.AddRandomLoreNote();
            Debug.Log($"[EnemyAI] Dropped Lore Note.");
        }
    }
}
