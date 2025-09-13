using UnityEngine;

public class PlayerDamageOverlay : MonoBehaviour
{
    public static PlayerDamageOverlay Instance { get; private set; }

    [Header("Overlay References")]
    [SerializeField] private CanvasGroup canvasGroup; 
    [SerializeField] private float fadeSpeed = 2f;    
    [SerializeField] private float maxFlashOpacity = 0.8f; // strongest possible flash
    [SerializeField] private float minFlashOpacity = 0.2f; // weakest flash at full HP

    private float targetAlpha = 0f;
    private float hpPercent = 1f; // track latest HP percentage (1 = full, 0 = dead)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f; 
    }

    private void Update()
    {
        // Smooth fade towards target alpha
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Snap back to 0 cleanly
        if (canvasGroup.alpha < 0.01f && targetAlpha == 0f)
            canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Should be called whenever HP changes.
    /// </summary>
    public void UpdateOverlayByHP(int currentHP, int maxHP)
    {
        hpPercent = Mathf.Clamp01((float)currentHP / maxHP);
    }

    public void FlashOnDamage()
    {
        // Flash strength scales with how low the HP is
        float flashStrength = Mathf.Lerp(minFlashOpacity, maxFlashOpacity, 1f - hpPercent);

        targetAlpha = flashStrength;

        CancelInvoke(nameof(ResetAlpha));
        Invoke(nameof(ResetAlpha), 0.1f);
    }

    private void ResetAlpha()
    {
        targetAlpha = 0f;
    }
}
