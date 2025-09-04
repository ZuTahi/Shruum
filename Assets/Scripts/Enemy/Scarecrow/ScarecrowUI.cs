using UnityEngine;
using TMPro;
using System.Collections;

public class ScarecrowUI : MonoBehaviour
{
    public static ScarecrowUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tipPanel;          // parent panel
    [SerializeField] private TextMeshProUGUI tipText;      // text inside

    [Header("Settings")]
    [SerializeField] private float tipDuration = 2.5f;     // how long text stays visible
    [SerializeField] private float delayBetweenTips = 1f;
    [SerializeField] private float fadeDuration = 0.5f;    // fade-out time

    private CanvasGroup canvasGroup;
    private bool tipActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (tipPanel != null)
        {
            // make sure it has a CanvasGroup
            canvasGroup = tipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = tipPanel.AddComponent<CanvasGroup>();

            // hide on load
            tipPanel.SetActive(false);
            if (tipText != null) tipText.gameObject.SetActive(false);

            canvasGroup.alpha = 0f;
        }
    }

    public void ShowTip(string text)
    {
        if (!tipActive && tipPanel != null && tipText != null)
            StartCoroutine(TipRoutine(text));
    }

    private IEnumerator TipRoutine(string text)
    {
        tipActive = true;

        tipPanel.SetActive(true);
        tipText.gameObject.SetActive(true);
        tipText.text = text;

        // appear instantly
        canvasGroup.alpha = 1f;

        // stay visible
        yield return new WaitForSeconds(tipDuration);

        // fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        // fully hidden
        canvasGroup.alpha = 0f;
        tipPanel.SetActive(false);
        tipText.gameObject.SetActive(false);
        tipText.text = string.Empty;

        // short delay before allowing new tip
        yield return new WaitForSeconds(delayBetweenTips);

        tipActive = false;
    }
}
