using UnityEngine;

public class Scarecrow : MonoBehaviour, IDamageable
{
    [Header("Tips")]
    [TextArea] public string[] tipLines;   // things he says when hit
    [Range(0f, 1f)] public float tipChance = 0.25f; // 25% chance

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage, Vector3 hitPoint, GameObject attacker)
    {
        if (animator != null)
            animator.SetTrigger("Hit"); // wobble animation

        // Try show a tip
        if (tipLines != null && tipLines.Length > 0 && Random.value < tipChance)
        {
            int idx = Random.Range(0, tipLines.Length);
            ScarecrowUI.Instance?.ShowTip(tipLines[idx]);
        }
    }
}
