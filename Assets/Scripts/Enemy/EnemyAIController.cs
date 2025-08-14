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

    // Animator parameters
    private static readonly int WALK_PARAM = Animator.StringToHash("isWalking");
    private static readonly int ATTACK_PARAM = Animator.StringToHash("attack");
    private static readonly int DIE_PARAM = Animator.StringToHash("die");

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
                StartCoroutine(WanderUntilAggro());
                break;
            case EnemyAIType.Ranged:
                StartCoroutine(RangedAI());
                break;
            case EnemyAIType.CollisionRetaliate:
                Debug.LogWarning($"{gameObject.name} AI type CollisionRetaliate not implemented yet.");
                break;
        }
    }

    private IEnumerator WanderUntilAggro()
    {
        while (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= data.awarenessRadius)
            {
                StartCoroutine(AggressiveAI());
                yield break;
            }

            Wander();
            yield return null;
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
                animator?.SetBool(WALK_PARAM, false);

                if (data.attackStyle == EnemyAttackStyle.Lunge)
                    yield return LungeAttackSequence();
                else
                    yield return NormalAttackSequence();
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
                animator?.SetBool(WALK_PARAM, false);
                yield return NormalAttackSequence();
            }

            yield return null;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        animator?.SetBool(WALK_PARAM, true);

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

    private void Wander()
    {
        animator?.SetBool(WALK_PARAM, true);

        if (agent != null && data.usesNavMesh)
        {
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 randomDir = Random.insideUnitSphere * 3f;
                randomDir += transform.position;
                if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }
        else
        {
            transform.position += Random.insideUnitSphere * data.moveSpeed * Time.deltaTime;
        }
    }

    private IEnumerator NormalAttackSequence()
    {
        animator?.SetTrigger(ATTACK_PARAM);
        yield return new WaitForSeconds(data.preAttackDuration);

        if (data.weaponPrefab != null)
        {
            GameObject hitbox = Instantiate(data.weaponPrefab, transform.position + transform.forward, transform.rotation);
            EnemyHitbox hb = hitbox.GetComponent<EnemyHitbox>();
            if (hb != null) hb.damage = data.attackDamage;
        }

        yield return new WaitForSeconds(data.attackDuration + data.postAttackDuration);
    }

    private IEnumerator LungeAttackSequence()
    {
        animator?.SetTrigger(ATTACK_PARAM);

        if (agent != null) agent.updateRotation = false;

        Vector3 direction = (player.position - transform.position).normalized;
        float launchSpeed = 18f;
        float launchDuration = 0.4f;
        float timer = 0f;
        bool damageApplied = false;

        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

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
                        }
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (agent != null) agent.updateRotation = true;
        yield return new WaitForSeconds(data.postAttackDuration);
    }

    public void TakeDamage(int amount, Vector3 hitPoint, GameObject source)
    {
        health -= amount;
        StartCoroutine(OnHitReaction());

        if (health <= 0) Die();
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
    }

    private void Die()
    {
        animator?.SetTrigger(DIE_PARAM);
        DropRewards();
        spawner?.OnEnemyDeath(gameObject);
        Destroy(gameObject, 0.5f); // allow death anim to play
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
        if (data != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.awarenessRadius);
        }
    }
}
