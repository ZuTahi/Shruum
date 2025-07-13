using System.Buffers.Text;
using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public float hpMultiplier = 1f;

    public int maxSP = 100;
    public int currentSP;
    public float spMultiplier = 1f;

    public int maxMP = 100;
    public int currentMP;
    public float mpMultiplier = 1f;

    public float attackDamage = 10f;
    public float attackMultiplier = 1f;

    public float baseDefensePercent = 0.0f; // 0 = 0%, 0.25 = 25%, etc.
    public float defenseMultiplier = 1f; // set by offering: x1.5 per mushroom

    public float TotalDefensePercent => Mathf.Clamp01(baseDefensePercent * defenseMultiplier);

    public float staminaRegenRate = 15f;
    private float spRegenBuffer = 0f;// SP per second

    public static PlayerStats Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentHP = maxHP;
        currentSP = maxSP;
        currentMP = maxMP;

        PlayerUIManager.Instance.UpdateHP(currentHP, maxHP);
        PlayerUIManager.Instance.UpdateSP(currentSP, maxSP);
        PlayerUIManager.Instance.UpdateMP(currentMP, maxMP);
    }

    void Update()
    {
        RegenerateStamina();
    }
    public void TakeDamage(int rawDamage)
    {
        float damageReduction = TotalDefensePercent;
        int finalDamage = Mathf.CeilToInt(rawDamage * (1f - damageReduction));
        finalDamage = Mathf.Max(finalDamage, 1); // minimum 1 damage

        currentHP -= finalDamage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log("Player has died.");
        }
        PlayerUIManager.Instance.UpdateHP(currentHP, maxHP);
    }

    void RegenerateStamina()
    {
        spRegenBuffer += staminaRegenRate * Time.deltaTime;
        if (spRegenBuffer >= 1f)
        {
            int regenAmount = Mathf.FloorToInt(spRegenBuffer);
            currentSP += regenAmount;
            spRegenBuffer -= regenAmount;

            if (currentSP > maxSP)
                currentSP = maxSP;
            PlayerUIManager.Instance.UpdateSP(currentSP, maxSP);
        }
    }
    public void RecalculateStats()
    {
        maxHP = Mathf.RoundToInt(maxHP * hpMultiplier);
        maxSP = Mathf.RoundToInt(maxSP * spMultiplier);
        maxMP = Mathf.RoundToInt(maxMP * mpMultiplier);
        attackDamage = attackDamage * attackMultiplier;
        baseDefensePercent = baseDefensePercent * defenseMultiplier;

        // Optional: cap current values
        currentHP = Mathf.Min(currentHP, maxHP);
        currentSP = Mathf.Min(currentSP, maxSP);
        currentMP = Mathf.Min(currentMP, maxMP);

        PlayerUIManager.Instance.UpdateHP(currentHP, maxHP);
        PlayerUIManager.Instance.UpdateSP(currentSP, maxSP);
        PlayerUIManager.Instance.UpdateMP(currentMP, maxMP);
    }
    public void SpendStamina(int amount)
    {
        currentSP = Mathf.Max(currentSP - amount, 0);
        PlayerUIManager.Instance.UpdateSP(currentSP, maxSP);
    }

    public bool HasEnoughStamina(int amount)
    {
        return currentSP >= amount;
    }
    public void SpendMana(int amount)
    {
        currentMP = Mathf.Max(currentMP - amount, 0);
        PlayerUIManager.Instance.UpdateMP(currentMP, maxMP);
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMP >= amount;
    }
    public void AddMana(int amount)
    {
        currentMP += amount;
        if (currentMP > maxMP)
            currentMP = maxMP;

        PlayerUIManager.Instance.UpdateMP(currentMP, maxMP);
        Debug.Log("Added Mana");
    }
}
