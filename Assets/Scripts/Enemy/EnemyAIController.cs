using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour, IDamageable
{
    public EnemyData data;
    public EnemySpawner spawner;

    private Transform player;
    private float health;
    private Animator animator;
    private Renderer rend;
    private Color originalColor;
    private NavMeshAgent agent;

    private bool isAggroed = false;
    private bool isStaggered = false;
    private float staggerDuration = 0.2f;

    private bool IsBloomedSpider => data.enemyName.ToLower().Contains("spider");

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

        StartCoroutine(IdleWanderUntilAggro());
    }

    private IEnumerator IdleWanderUntilAggro()
    {
        if (!data.enableWanderInIdle || agent == null)
            yield break;

        while (!isAggroed && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= data.awarenessRadius)
            {
                Aggro();
                yield break;
            }

            // Wander near spawn
            Vector2 offset = Random.insideUnitCircle * data.idleWanderRadius;
            Vector3 target = transform.position + new Vector3(offset.x, 0f, offset.y);

            agent.speed = data.moveSpeed;
            agent.SetDestination(target);

            float timer = 0f;
            while (timer < data.idleWanderInterval && !isAggroed)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            agent.ResetPath();
        }
    }

    private void Aggro()
    {
        isAggroed = true;
        animator?.SetTrigger("Alert");

        // Group awareness
        if (data.groupAwarenessRadius > 0f)
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, data.groupAwarenessRadius);
            foreach (var col in nearby)
            {
                EnemyAIController other = col.GetComponent<EnemyAIController>();
                if (other != null && !other.isAggroed)
                {
                    other.Aggro();
                }
            }
        }

        if (data.aiType == EnemyAIType.Aggressive)
            StartCoroutine(AggressiveAI());
        else if (data.aiType == EnemyAIType.Ranged)
            StartCoroutine(RangedAI());
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
                if (IsBloomedSpider)
                    yield return LaunchAttackSequence();
                else
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
                hb.damage = data.attackDamage;
        }

        // Wander slightly during post attack
        if (data.enableWanderInIdle)
        {
            Vector2 offset = Random.insideUnitCircle * 0.75f;
            Vector3 target = transform.position + new Vector3(offset.x, 0, offset.y);
            if (agent != null)
            {
                agent.speed = data.moveSpeed;
                agent.SetDestination(target);
            }
        }

        yield return new WaitForSeconds(data.postAttackDuration);
    }

    private IEnumerator LaunchAttackSequence()
    {
        animator?.SetTrigger("Lunge");

        if (agent != null)
            agent.updateRotation = false;

        Vector3 direction = (player.position - transform.position).normalized;

        float launchSpeed = 18f;
        float launchDuration = 0.5f;
        float timer = 0f;
        bool damageApplied = false;

        transform.rotation = Quaternion.LookRotation(direction);

        while (timer < launchDuration)
        {
            transform.position += direction * launchSpeed * Time.deltaTime;

            if (!damageApplied)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, 1.0f);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        PlayerStats ps = hit.GetComponent<PlayerStats>();
                        if (ps != null)
                        {
                            ps.TakeDamage(data.attackDamage);
                            damageApplied = true;
                            break;
                        }
                    }
                }
                if (damageApplied) break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (agent != null)
            agent.updateRotation = true;

        yield return new WaitForSeconds(data.postAttackDuration);
    }

    public void TakeDamage(int amount, Vector3 hitPoint, GameObject source)
    {
        health -= amount;
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

        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        rend.material.color = originalColor;
        yield return new WaitForSeconds(0.05f);
        rend.material.color = Color.red;
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
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (data == null)
            return;

        // Awareness radius (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.awarenessRadius);

        // Group awareness (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.groupAwarenessRadius);

        // Idle wander (cyan)
        if (data.enableWanderInIdle)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, data.idleWanderRadius);
        }
    }

}
