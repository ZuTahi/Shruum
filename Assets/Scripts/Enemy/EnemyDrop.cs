using UnityEngine;
using System.Collections;

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop Prefabs")]
    public GameObject manaDropPrefab;

    [Header("Drop Values")]
    public int manaAmount = 1;

    [Header("Spawn Tuning")]
    public float dropSpreadRadius = 0.25f;   // smaller ring around enemy
    public float dropUpwardForce = 0.35f;    // now treated as gentle pop velocity (m/s)
    public float ignoreEnemyCollisionSeconds = 0.25f;
    public float groundProbeHeight = 1.5f;   // how far above we raycast from
    public float spawnLift = 0.06f;          // small lift to prevent ground clipping

    public void SetDropValues(int mana)
    {
        manaAmount = mana;
    }

    public void DropItems()
    {
        // Find ground right under the enemy so we can place items on it.
        Vector3 basePos = transform.position + Vector3.up * groundProbeHeight;
        if (Physics.Raycast(basePos, Vector3.down, out var hit, groundProbeHeight + 3f, 
            ~0, QueryTriggerInteraction.Ignore))
        {
            basePos = hit.point;
        }
        else
        {
            // Fallback if no ground hit; still try to keep it reasonable.
            basePos = transform.position;
        }

        // Cache enemy colliders to temporarily ignore.
        var enemyCols = GetComponentsInChildren<Collider>();

        // Spawn mana
        for (int i = 0; i < manaAmount; i++)
        {
            Vector2 planar = Random.insideUnitCircle * dropSpreadRadius;
            Vector3 pos = basePos + new Vector3(planar.x, spawnLift, planar.y);
            SpawnOne(manaDropPrefab, pos, enemyCols);
        }
    }

    private void SpawnOne(GameObject prefab, Vector3 position, Collider[] enemyCols)
    {
        if (prefab == null) return;

        var go = Instantiate(prefab, position, Quaternion.identity);

        // Gentle, Tunic-like pop + quick settling.
        if (go.TryGetComponent<Rigidbody>(out var rb))
        {
            // Make sure we’re starting from calm state.
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // A touch of damping helps prevent long rolls.
            if (rb.linearDamping < 0.2f) rb.linearDamping = 0.2f;
            if (rb.angularDamping < 0.2f) rb.angularDamping = 0.2f;

            // Clamp de-penetration kick so nothing rockets up from overlap correction.
#if UNITY_2021_2_OR_NEWER
            if (rb.maxDepenetrationVelocity > 2f) rb.maxDepenetrationVelocity = 2f;
#endif

            // Small upward + tiny sideways “pop” using velocity change (frame-rate independent).
            float upV = Mathf.Clamp(dropUpwardForce, 0f, 2f);      // m/s
            float sideV = upV * 0.35f;                              // a fraction sideways
            Vector2 lateral = Random.insideUnitCircle.normalized * sideV;
            Vector3 v = new Vector3(lateral.x, upV, lateral.y);

            rb.AddForce(v, ForceMode.VelocityChange);
        }

        // Avoid explosive push from overlapping the enemy for a brief moment.
        if (go.TryGetComponent<Collider>(out var itemCol) && enemyCols != null && enemyCols.Length > 0)
        {
            StartCoroutine(TemporarilyIgnore(enemyCols, itemCol, ignoreEnemyCollisionSeconds));
        }
    }

    private IEnumerator TemporarilyIgnore(Collider[] enemyCols, Collider itemCol, float seconds)
    {
        foreach (var ec in enemyCols)
            if (ec) Physics.IgnoreCollision(ec, itemCol, true);

        yield return new WaitForSeconds(seconds);

        foreach (var ec in enemyCols)
            if (ec) Physics.IgnoreCollision(ec, itemCol, false);
    }
}
