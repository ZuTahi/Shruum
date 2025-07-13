using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyAttack))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(Animator))]
public class EnemyAIController : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, WindUp, Attack, Recover, Death }
    public EnemyState currentState = EnemyState.Idle;

    public float detectionRange = 6f;
    public float attackRange = 2f;
    public GameObject deathPoofVFX;

    private Transform player;
    private EnemyStats stats;
    private EnemyAttack attack;
    private EnemyMovement movement;
    private Animator anim;

    private Quaternion lockedRotation;
    private bool isActing = false;
    private bool isStaggered = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        stats = GetComponent<EnemyStats>();
        attack = GetComponent<EnemyAttack>();
        movement = GetComponent<EnemyMovement>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (stats.IsDead || stats.isDummy || isActing || isStaggered || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                anim.SetBool("isWalking", false);
                if (dist <= detectionRange)
                    currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                anim.SetBool("isWalking", true);
                movement.MoveTo(player.position);
                FacePlayer();
                if (dist <= attackRange)
                {
                    movement.Stop();
                    currentState = EnemyState.WindUp;
                }
                break;

            case EnemyState.WindUp:
                StartCoroutine(WindUpAttackSequence());
                break;

            case EnemyState.Death:
                break;
        }
    }

    private IEnumerator WindUpAttackSequence()
    {
        isActing = true;
        movement.Stop();
        float timer = 0f;
        anim.SetTrigger("attack");

        while (timer < attack.windUpTime)
        {
            FacePlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        lockedRotation = transform.rotation;
        currentState = EnemyState.Attack;
        StartCoroutine(AttackAndRecover());
    }

    private IEnumerator AttackAndRecover()
    {
        transform.rotation = lockedRotation;
        attack.SpawnHitbox();
        yield return new WaitForSeconds(0.1f);

        yield return new WaitForSeconds(attack.attackRecoveryTime);
        isActing = false;
        currentState = EnemyState.Chase;
    }

    private void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 20f);
        }
    }

    // 🔥 NEW: Stagger Reaction on Hit
    public void OnHitReaction()
    {
        if (currentState == EnemyState.Death || isStaggered) return;

        // Cancel attack sequence if in WindUp
        if (currentState == EnemyState.WindUp)
        {
            StopAllCoroutines();
            isActing = false;
            currentState = EnemyState.Chase;
        }

        StartCoroutine(HitStagger());
    }

    private IEnumerator HitStagger()
    {
        isStaggered = true;
        movement.Stop();
        anim.SetTrigger("hit"); // optional: add "hit" trigger in Animator
        yield return new WaitForSeconds(0.15f);
        isStaggered = false;
    }

    public void OnDeath()
    {
        if (stats.isDummy) return;
        GetComponent<EnemyDrop>()?.DropItems();

        currentState = EnemyState.Death;
        movement.Stop();
        anim.SetTrigger("die");
        StartCoroutine(DisappearWithPoof());
    }

    private IEnumerator DisappearWithPoof()
    {
        yield return new WaitForSeconds(0.5f);
        if (deathPoofVFX != null)
            Instantiate(deathPoofVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attack != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 pos = attack.attackOrigin
                ? attack.attackOrigin.position
                : transform.position + transform.forward * attack.hitboxOffset;
            Gizmos.DrawWireSphere(pos, 0.5f);
        }
    }
}
