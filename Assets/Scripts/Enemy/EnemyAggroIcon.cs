using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyAggroIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage; // assign in prefab (e.g. "!" sprite)
    [SerializeField] private float visibleTime = 1f;
    [SerializeField] private float fadeInDuration = 0.15f;   // quick
    [SerializeField] private float fadeOutDuration = 0.6f;   // slow
    [SerializeField] private float popupHeight = 0.5f;       // how high it moves up
    [SerializeField] private AnimationCurve popupCurve = AnimationCurve.EaseInOut(0,0,1,1);

    private Transform target;
    private Vector3 baseOffset;
    private Vector3 offset;
    private Quaternion fixedRotation;
    private Coroutine fadeRoutine;

    // Attach to enemy transform (like health bar)
    public void AttachTo(Transform targetTransform, Vector3 offsetPosition)
    {
        target = targetTransform;
        baseOffset = offsetPosition;
        offset = offsetPosition;

        if (Camera.main != null)
            fixedRotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        else
            fixedRotation = Quaternion.identity;

        if (iconImage != null)
        {
            var c = iconImage.color;
            c.a = 0f;
            iconImage.color = c;
        }
        gameObject.SetActive(false);
    }

    public void Show()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        gameObject.SetActive(true);
        fadeRoutine = StartCoroutine(FadeRoutine()); // fade in → hold → fade out
    }

    private IEnumerator FadeRoutine()
    {
        if (iconImage == null) yield break;

        Color c = iconImage.color;

        // --- fade in with upward "pop" ---
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float p = t / fadeInDuration;

            c.a = Mathf.Lerp(0f, 1f, p);
            iconImage.color = c;

            // move offset up with curve
            offset = baseOffset + Vector3.up * popupHeight * popupCurve.Evaluate(p);

            yield return null;
        }

        // --- hold ---
        yield return new WaitForSeconds(visibleTime);

        // --- fade out slowly ---
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float p = t / fadeOutDuration;

            c.a = Mathf.Lerp(1f, 0f, p);
            iconImage.color = c;
            yield return null;
        }

        // reset and disable
        offset = baseOffset;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = fixedRotation; // same trick as health bar
    }
}
