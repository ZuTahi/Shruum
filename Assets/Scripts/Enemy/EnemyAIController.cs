using System.Collections.Generic;
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
    private Renderer[] renderers;
    private Dictionary<Renderer, Color[]> originalColors = new Dictionary<Renderer, Color[]>();

    private bool hasAggro = false;

    [Header("UI")]
    public EnemyHealthBar healthBarPrefab;
    private EnemyHealthBar healthBarInstance;

    [Header("Aggro")]
    [SerializeField] private EnemyAggroIcon aggroIconPrefab;
    private EnemyAggroIcon aggroIconInstance;
    // Add near the top with other fields
    private bool isLunging = false;
    [SerializeField] private float lungeSpeed = 18f;
    
    [Header("Drops")]
    public EnemyDrop enemyDropPrefab;
    public int minManaDrop = 0;
    public int maxManaDrop = 2;

    [Header("VFX")]
    public GameObject poofPrefab;   // assign in inspector (TUNIC-style poof effect)
    private bool isStaggered = false;
    private float staggerDuration = 0.2f;

    [Header("Audio")]
    public AudioClip hitClip;

    private AudioSource audioSource;
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

    // ========== WANDER SUB-STATE ==========
    private enum WanderSubstate { Waiting, Turning, Moving }
    private WanderSubstate wanderSub = WanderSubstate.Waiting;

    [Header("Wander Settings")]
    [SerializeField] private float minWanderWait = 1f;
    [SerializeField] private float maxWanderWait = 3f;
    [SerializeField] private float wanderRadius = 3.5f;
    [SerializeField] private float minWanderDistance = 1.0f; // avoid picking very close points
    [SerializeField] private float turnSpeedDegPerSec = 360f;
    [SerializeField] private float arriveTolerance = 0.15f;

    private float wanderTimer = 0f;
    private Vector3 wanderTarget;
    private bool hasWanderTarget = false;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.7f; // 3D positional audio
        // Get all renderers in this enemy (including children)
        renderers = GetComponentsInChildren<Renderer>();

        // Cache original colors
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;
            Color[] colors = new Color[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                colors[i] = mats[i].color;
            }
            originalColors[r] = colors;
        }
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
            if (agent != null)
            {
                agent.speed = data.moveSpeed;
                agent.updateRotation = false;
                agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, 0.05f);
                agent.autoBraking = true;
            }
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();

        health = data.maxHealth;

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab);
            healthBarInstance.AttachTo(transform, new Vector3(0f, 2f, 0f));
            healthBarInstance.SetMaxHealth(health);
        }

        if (aggroIconPrefab != null)
        {
            aggroIconInstance = Instantiate(aggroIconPrefab);
            aggroIconInstance.AttachTo(transform, new Vector3(0f, 2.5f, 0f)); // above head
        }

        EnterWanderWaiting();
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
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= data.awarenessRadius)
        {
            aggroIconInstance?.Show();
            if (agent != null) agent.ResetPath();
            currentState = EnemyState.Chase;
            return;
        }
        if (!hasAggro && distToPlayer <= data.awarenessRadius)
        {
            hasAggro = true;  // 🟢 lock aggro forever
            aggroIconInstance?.Show();
            if (agent != null) agent.ResetPath();
            currentState = EnemyState.Chase;
            return;
        }
        WanderMovementStep();
    }

    private void UpdateChase()
    {
        if (player == null) return;

        // 🟢 Always ensure agent is active when chasing
        if (agent != null && data.usesNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (agent.velocity.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(agent.velocity.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeedDegPerSec * Time.deltaTime);
            }

            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
        else
        {
            Vector3 dir = (player.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
                transform.position += transform.forward * data.moveSpeed * Time.deltaTime;

                animator.SetFloat("Speed", data.moveSpeed);
            }
        }

        // 🟢 Enter Windup → Attack if in range
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= data.attackRange)
        {
            if (agent != null)
            {
                agent.ResetPath();
                agent.isStopped = true;
            }

            currentState = EnemyState.Windup;
            stateTimer = data.preAttackDuration;  // telegraph time
            animator.SetFloat("Speed", 0f);
        }
    }
    // Called by animation event at the start of the lunge motion
    public void StartLunge()
    {
        isLunging = true;
        damageApplied = false;
        currentState = EnemyState.Attack;   // ✅ now switch when the animation actually begins
        stateTimer = data.attackDuration;
    }

    // Called by animation event at the end of the lunge motion
    public void EndLunge()
    {
        isLunging = false;

        // Put into PostAttack "rest" state
        currentState = EnemyState.PostAttack;
        stateTimer = 1.5f; // 1.5s cooldown after lunge
    }

    private void UpdateWindup()
    {
        FacePlayer(); // lock in facing while winding up

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

                // 🛑 Lock the facing here so it won't keep adjusting in UpdateAttack
                FacePlayer();
                animator.SetTrigger("attack");
            }
            else
            {
                animator.SetTrigger("attack");
                currentState = EnemyState.Attack;
                stateTimer = data.attackDuration;
                damageApplied = false;
                
            }
        }
    }

    private void UpdateAttack()
    {
        if (data.attackStyle == EnemyAttackStyle.Lunge)
        {
            // ❌ old: LungeForward();
            if (isLunging)
            {
                // Move forward while animation says so
                transform.position += transform.forward * lungeSpeed * Time.deltaTime;

                // Hit detection
                if (!damageApplied)
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.2f, 0.8f);
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
        }
        else
        {
            MeleeAttack();
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = EnemyState.PostAttack;
            stateTimer = data.postAttackDuration;

            animator.SetFloat("Speed", 0f);

            if (agent != null)
            {
                agent.Warp(transform.position);
                agent.isStopped = false;
                agent.updatePosition = true;
                agent.updateRotation = false;
            }
        }
    }

    private void UpdatePostAttack()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {

            currentState = EnemyState.Chase;
        }
    }
    // ------------------- WANDER SUB-STATE MACHINE -------------------

    private void EnterWanderWaiting()
    {
        wanderSub = WanderSubstate.Waiting;
        wanderTimer = Random.Range(minWanderWait, maxWanderWait);
        hasWanderTarget = false;

        if (agent != null) agent.ResetPath();

        animator.SetFloat("Speed", 0f);
    }

    private void PickWanderTarget()
    {
        const int attempts = 6;
        Vector3 basePos = transform.position;

        for (int i = 0; i < attempts; i++)
        {
            Vector3 candidate = basePos + Random.insideUnitSphere * wanderRadius;
            candidate.y = basePos.y;

            if (data.usesNavMesh && agent != null)
            {
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                {
                    if ((hit.position - basePos).magnitude >= minWanderDistance)
                    {
                        wanderTarget = hit.position;
                        hasWanderTarget = true;
                        return;
                    }
                }
            }
            else
            {
                if ((candidate - basePos).magnitude >= minWanderDistance)
                {
                    wanderTarget = candidate;
                    hasWanderTarget = true;
                    return;
                }
            }
        }

        // fallback: just wait again if we couldn't find a good point
        hasWanderTarget = false;
    }

    private void WanderMovementStep()
    {
        switch (wanderSub)
        {
            case WanderSubstate.Waiting:
                wanderTimer -= Time.deltaTime;
                animator.SetFloat("Speed", 0f);

                if (wanderTimer <= 0f)
                {
                    PickWanderTarget();
                    if (hasWanderTarget)
                        wanderSub = WanderSubstate.Turning;
                    else
                        EnterWanderWaiting(); // try again later
                }
                break;

            case WanderSubstate.Turning:
            {
                Vector3 toTarget = wanderTarget - transform.position;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude < 0.001f)
                {
                    // Target is basically on top of us; pick another
                    EnterWanderWaiting();
                    break;
                }

                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    turnSpeedDegPerSec * Time.deltaTime
                );

                animator.SetFloat("Speed", 0f);

                float angle = Vector3.Angle(transform.forward, toTarget);
                if (angle <= 5f)
                {
                    // Start moving after we’re facing it
                    if (agent != null && data.usesNavMesh)
                    {
                        agent.SetDestination(wanderTarget);
                    }
                    wanderSub = WanderSubstate.Moving;
                }
                break;
            }

            case WanderSubstate.Moving:
            {
                if (agent != null && data.usesNavMesh)
                {
                    // Smoothly face movement direction
                    if (agent.velocity.sqrMagnitude > 0.0001f)
                    {
                        Quaternion faceVel = Quaternion.LookRotation(agent.velocity.normalized);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, faceVel, turnSpeedDegPerSec * Time.deltaTime);
                    }

                    float speed = agent.velocity.magnitude;
                    animator.SetFloat("Speed", speed);

                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + arriveTolerance)
                    {
                        EnterWanderWaiting();
                    }
                }
                else
                {
                    // Fallback no-NavMesh wander move
                    Vector3 toTarget = wanderTarget - transform.position;
                    toTarget.y = 0f;

                    if (toTarget.sqrMagnitude < 0.04f)
                    {
                        EnterWanderWaiting();
                        break;
                    }

                    Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
                    transform.position += transform.forward * (data.moveSpeed * 0.6f) * Time.deltaTime;

                    animator.SetFloat("Speed", data.moveSpeed * 0.6f);
                }
                break;
            }
        }
    }

    // ------------------- GENERAL MOVEMENT / FACING -------------------

    private void MoveTowards(Vector3 target)
    {
        if (agent != null && data.usesNavMesh)
        {
            agent.SetDestination(target);

            if (agent.velocity.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
            }

            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
        else
        {
            Vector3 dir = (target - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
                transform.position += transform.forward * data.moveSpeed * Time.deltaTime;

                animator.SetFloat("Speed", data.moveSpeed);
            }
        }
    }

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
        }
    }

    // ------------------- ATTACK METHODS -------------------

    // private void LungeForward()
    // {
    //     if (!damageApplied)
    //     {
    //         // Hitbox shifted forward (head area), tighter radius
    //         Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.2f, 0.8f);
    //         foreach (var hit in hits)
    //         {
    //             if (hit.CompareTag("Player"))
    //             {
    //                 var ps = hit.GetComponent<PlayerStats>();
    //                 if (ps != null) ps.TakeDamage(data.attackDamage);

    //                 damageApplied = true;

    //                 // stop lunge immediately on hit
    //                 stateTimer = 0f; 
    //                 return;
    //             }
    //         }
    //     }

    //     // keep moving forward only while in attack state
    //     if (!damageApplied)
    //         transform.position += transform.forward * 18f * Time.deltaTime;
    // }

    private void MeleeAttack()
    {
        FacePlayer();
    }
    // Called by Animation Event at the strike frame
// Called by Animation Event at the strike frame
    public void OnMeleeHit()
    {
        if (damageApplied) return;

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward, 1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var ps = hit.GetComponent<PlayerStats>();
                if (ps != null)
                {
                    ps.TakeDamage(data.attackDamage);
                }

                damageApplied = true;
                break;
            }
        }
    }

    // ------------------- DAMAGE -------------------
    public void TakeDamage(int amount, Vector3 hitPoint, GameObject source)
    {
        health -= amount;
        hasAggro = true;
        currentState = EnemyState.Chase; 
        if (healthBarInstance != null)
        {
            healthBarInstance.Show();             // 🆕 make it visible
            healthBarInstance.SetHealth(health);
        }
        if (hitClip != null && health > 0f)
            audioSource.PlayOneShot(hitClip);

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

    public IEnumerator FlickerRed(float duration = 0.1f, int times = 2)
    {
        for (int t = 0; t < times; t++)
        {
            // Set all to red
            foreach (Renderer r in renderers)
            {
                foreach (Material mat in r.materials)
                {
                    mat.color = Color.red;
                }
            }

            yield return new WaitForSeconds(duration);

            // Restore original colors
            foreach (Renderer r in renderers)
            {
                Color[] colors = originalColors[r];
                Material[] mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].color = colors[i];
                }
            }

            yield return new WaitForSeconds(duration);
        }
    }
    private void Die()
    {
        // 🟢 Spawn poof effect immediately
        if (poofPrefab != null)
        {
            GameObject poof = Instantiate(poofPrefab, transform.position, Quaternion.identity);
            Destroy(poof, 1.5f); // clean up particle system after it finishes
        }

        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject);
        if (aggroIconInstance != null)               // 🆕 cleanup
            Destroy(aggroIconInstance.gameObject);
        DropRewards();
        spawner?.OnEnemyDeath(gameObject);

        Destroy(gameObject); // destroy enemy immediately after spawning poof
    }

    private void DropRewards()
    {
        if (enemyDropPrefab != null)
        {
            var drop = Instantiate(enemyDropPrefab, transform.position, Quaternion.identity);
            
            // Only drop mana
            int mana = Random.Range(minManaDrop, maxManaDrop + 1);
            drop.SetDropValues(mana);   // updated EnemyDrop script should only take mana now
            drop.DropItems();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        // Awareness + Attack range (already there)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.awarenessRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);

        // 🟢 Show Lunge hitbox
        if (data.attackStyle == EnemyAttackStyle.Lunge)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, 1f);
        }

        // 🟢 Show Melee hitbox
        if (data.attackStyle == EnemyAttackStyle.NormalMelee)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position + transform.forward, .5f);
        }
    }

}
