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
    private NavMeshAgent agent;
    private Renderer rend;
    private Color originalColor;

    [Header("UI")]
    public EnemyHealthBar healthBarPrefab;
    private EnemyHealthBar healthBarInstance;

    [Header("Drops")]
    public EnemyDrop enemyDropPrefab;
    public int minNatureForceDrop = 1;
    public int maxNatureForceDrop = 3;
    public int minManaDrop = 0;
    public int maxManaDrop = 2;
    [Range(0f, 1f)] public float loreNoteDropChance = 0.15f;

    private bool isStaggered = false;
    private float staggerDuration = 0.2f;

    private EnemyState currentState;
    private float stateTimer;
    private bool damageApplied;

    private enum EnemyState
    {
        Wander,
        Chase,
        Windup,
        Attack,
        PostAttack
    }

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyAIController] No EnemyData on {name}");
            Destroy(gameObject);
            return;
        }

        if (data.usesNavMesh)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.speed = data.moveSpeed;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rend = GetComponentInChildren<Renderer>();
        if (rend != null) originalColor = rend.material.color;

        health = data.maxHealth;

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab);
            healthBarInstance.AttachTo(transform, new Vector3(0f, 2f, 0f));
            healthBarInstance.SetMaxHealth(health);
        }

        currentState = EnemyState.Wander;
    }

    private void Update()
    {
        if (player == null || isStaggered) return;

        switch (currentState)
        {
            case EnemyState.Wander:
                UpdateWander();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Windup:
                UpdateWindup();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.PostAttack:
                UpdatePostAttack();
                break;
        }
    }

    // ------------------- STATE UPDATES -------------------

    private void UpdateWander()
    {
        animator.SetBool("isWalking", true);

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= data.awarenessRadius)
        {
            currentState = EnemyState.Chase;
            return;
        }

        WanderMovement();
    }

    private void UpdateChase()
    {
        animator.SetBool("isWalking", true);

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= data.attackRange)
        {
            currentState = EnemyState.Windup;
            stateTimer = data.preAttackDuration;
            animator.SetBool("isWalking", false);

            if (data.attackStyle == EnemyAttackStyle.Lunge)
                animator.SetTrigger("windup");
            else
                animator.SetTrigger("attack"); // normal melee windup (optional trigger)
            return;
        }

        MoveTowards(player.position);
    }

    private void UpdateWindup()
    {
        FacePlayer();
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentState = EnemyState.Attack;
            stateTimer = data.attackDuration;
            damageApplied = false;

            if (data.attackStyle == EnemyAttackStyle.Lunge)
            {
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                }
                animator.SetTrigger("attack");
            }
            else
            {
                animator.SetTrigger("attack");
            }
        }
    }

    private void UpdateAttack()
    {
        if (data.attackStyle == EnemyAttackStyle.Lunge)
            LungeForward();
        else
            MeleeAttack();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = EnemyState.PostAttack;
            stateTimer = data.postAttackDuration;
            animator.SetBool("isWalking", false);

            if (agent != null)
            {
                agent.Warp(transform.position);
                agent.isStopped = false;
                agent.updatePosition = true;
                agent.updateRotation = true;
            }
        }
    }

    private void UpdatePostAttack()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            currentState = EnemyState.Chase;
    }

    // ------------------- MOVEMENT -------------------

    private void WanderMovement()
    {
        if (agent != null && data.usesNavMesh)
        {
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 random = Random.insideUnitSphere * 3f + transform.position;
                if (NavMesh.SamplePosition(random, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }
        }
        else
        {
            Vector3 dir = Random.insideUnitSphere;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 180f * Time.deltaTime);
                transform.position += transform.forward * data.moveSpeed * 0.25f * Time.deltaTime;
            }
        }
    }

    private void MoveTowards(Vector3 target)
    {
        if (agent != null && data.usesNavMesh)
        {
            agent.SetDestination(target);
        }
        else
        {
            Vector3 dir = (target - transform.position);
            dir.y = 0f;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 360f * Time.deltaTime);
            transform.position += transform.forward * data.moveSpeed * Time.deltaTime;
        }
    }

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 360f * Time.deltaTime);
        }
    }

    // ------------------- ATTACK METHODS -------------------

    private void LungeForward()
    {
        transform.position += transform.forward * 18f * Time.deltaTime;

        if (!damageApplied)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 1f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var ps = hit.GetComponent<PlayerStats>();
                    if (ps != null) ps.TakeDamage(data.attackDamage);
                    damageApplied = true;
                    break;
                }
            }
        }
    }

    private void MeleeAttack()
    {
        if (!damageApplied)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward, 1f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var ps = hit.GetComponent<PlayerStats>();
                    if (ps != null) ps.TakeDamage(data.attackDamage);
                    damageApplied = true;
                    break;
                }
            }
        }
    }

    // ------------------- DAMAGE -------------------

    public void TakeDamage(int amount, Vector3 hitPoint, GameObject source)
    {
        health -= amount;
        healthBarInstance?.SetHealth(health);
        StartCoroutine(OnHitReaction());

        if (health <= 0f) Die();
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
        for (int i = 0; i < 2; i++)
        {
            rend.material.color = red;
            yield return new WaitForSeconds(0.05f);
            rend.material.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void Die()
    {
        animator?.SetTrigger("die");

        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject, 0.5f);

        DropRewards();
        spawner?.OnEnemyDeath(gameObject);
        Destroy(gameObject, 0.5f);
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
            PlayerInventory.Instance?.AddRandomLoreNote();
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.awarenessRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}
