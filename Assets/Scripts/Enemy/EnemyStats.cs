using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour, IDamageable
{
    public int maxHP = 100;
    public int currentHP;
    public int attackDamage = 10;
    public bool isDummy = false;

    private Coroutine flickerCoroutine;
    private Renderer rend;
    private Color originalColor;

    public bool IsDead => currentHP <= 0;

    private void Awake()
    {
        currentHP = maxHP;
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    // CONTROLLER
    public void TakeDamage(int amount, Vector3 hitPoint, GameObject source)
    {
        // Trigger flashing
        if (rend != null)
        {
            if (flickerCoroutine != null)
                StopCoroutine(flickerCoroutine);

            flickerCoroutine = StartCoroutine(FlickerSequence());
        }

        // Trigger stagger (fake knockback)
        GetComponent<EnemyAIController>()?.OnHitReaction();

        if (isDummy) return;

        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
        }
    }


    private IEnumerator FlickerSequence()
    {
        Color red = Color.red;
        Color white = Color.white;

        for (int i = 0; i < 2; i++)
        {
            rend.material.color = red;
            yield return new WaitForSeconds(0.05f);
            rend.material.color = white;
            yield return new WaitForSeconds(0.05f);
        }

        rend.material.color = originalColor;
    }

    public void ResetHP()
    {
        currentHP = maxHP;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }
}
