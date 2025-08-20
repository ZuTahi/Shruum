using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int maxHP;
    public int currentHP;

    public int maxSP;
    public int currentSP;

    public int maxMP;
    public int currentMP;

    public float attackDamage;

    public float baseDefensePercent;
    public float defenseMultiplier;

    public float attackMultiplier;
    public float staminaRegenRate;
    private float spRegenBuffer = 0f;

    public float TotalDefensePercent => Mathf.Clamp01(baseDefensePercent * defenseMultiplier);
    private bool isDead = false;
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

    private void Start()
    {
        LoadFromData(); // now preserves ratios AND applies run buffs
        ModularWeaponSlotManager.Instance?.ApplyEquippedWeaponsFromPlayerData();
        WeaponEquipUI.Instance?.RefreshAllSlots();
        Debug.Log("[PlayerStats] Loaded data and applied run buffs.");
    }

    private void Update()
    {
        RegenerateStamina();
    }
    public void LoadFromData()
    {
        // Capture old ratios and "was full" flags
        float hpRatio = (maxHP > 0) ? (float)currentHP / maxHP : 1f;
        bool hpWasFull = (maxHP > 0) && (currentHP >= maxHP);

        float spRatio = (maxSP > 0) ? (float)currentSP / maxSP : 1f;
        bool spWasFull = (maxSP > 0) && (currentSP >= maxSP);

        float mpRatio = (maxMP > 0) ? (float)currentMP / maxMP : 1f;
        bool mpWasFull = (maxMP > 0) && (currentMP >= maxMP);

        // Load permanent baseline
        maxHP = PlayerData.maxHP;
        maxSP = PlayerData.maxSP;
        maxMP = PlayerData.maxMP;

        attackMultiplier     = PlayerData.attackMultiplier;
        baseDefensePercent   = PlayerData.baseDefensePercent;
        defenseMultiplier    = PlayerData.defenseMultiplier;

        // Apply run buffs (adds bonusHP, etc.)
        RunData.ApplyRunBuffs(this);  // 

        // Restore currents: if it was full, keep it full; else keep the same ratio
        currentHP = hpWasFull ? maxHP : Mathf.RoundToInt(hpRatio * maxHP);
        currentSP = spWasFull ? maxSP : Mathf.RoundToInt(spRatio * maxSP);
        currentMP = mpWasFull ? maxMP : Mathf.RoundToInt(mpRatio * maxMP);

        RefreshUI();
        Debug.Log("[PlayerStats] Loaded from PlayerData (ratios preserved).");
    }

    public void TakeDamage(int rawDamage)
    {
        if (isDead) return;  // Already dead

        float damageReduction = TotalDefensePercent;
        int finalDamage = Mathf.CeilToInt(rawDamage * (1f - damageReduction));
        finalDamage = Mathf.Max(finalDamage, 1);

        currentHP -= finalDamage;
        CameraShake.Instance?.Shake(0.2f, 0.15f);
        if (currentHP <= 0)
        {
            currentHP = 0;
            isDead = true;
            Debug.Log("Player has died.");
            GameManager.Instance.RespawnAtHub();
        }
        PlayerUIManager.Instance.UpdateHP(currentHP, maxHP);
    }

    private void RegenerateStamina()
    {
        spRegenBuffer += staminaRegenRate * Time.deltaTime;
        if (spRegenBuffer >= 1f)
        {
            int regenAmount = Mathf.FloorToInt(spRegenBuffer);
            currentSP = Mathf.Min(currentSP + regenAmount, maxSP);
            spRegenBuffer -= regenAmount;

            PlayerUIManager.Instance.UpdateSP(currentSP, maxSP);
        }
    }

    public void SpendStamina(int amount)
    {
        currentSP = Mathf.Max(currentSP - amount, 0);
        PlayerUIManager.Instance.UpdateSP(currentSP, maxSP);
    }

    public bool HasEnoughStamina(int amount) => currentSP >= amount;

    public void SpendMana(int amount)
    {
        currentMP = Mathf.Max(currentMP - amount, 0);
        PlayerUIManager.Instance.UpdateMP(currentMP, maxMP);
    }

    public bool HasEnoughMana(int amount) => currentMP >= amount;

    public void AddMana(int amount)
    {
        currentMP = Mathf.Min(currentMP + amount, maxMP);
        PlayerUIManager.Instance.UpdateMP(currentMP, maxMP);
    }

    // ✅ Stat Increases
    public void IncreaseMaxHP(int amount)
    {
        maxHP += amount;
        currentHP += amount;
        PlayerUIManager.Instance.UpdateHP(currentHP, maxHP);
    }

    public void IncreaseMaxSP(int amount)
    {
        maxSP += amount;
        currentSP += amount;
        PlayerUIManager.Instance.UpdateSP(currentSP, maxSP);
    }

    public void IncreaseMaxMP(int amount)
    {
        maxMP += amount;
        currentMP += amount;
        PlayerUIManager.Instance.UpdateMP(currentMP, maxMP);
    }

    public void RefreshUI()
    {
        PlayerUIManager.Instance?.RefreshAllStats();
    }

    public void RevivePlayer()
    {
        isDead = false;
        currentHP = maxHP;
        currentSP = maxSP;
        currentMP = maxMP;
        RefreshUI();
        Debug.Log("[PlayerStats] Player revived.");
    }

}
