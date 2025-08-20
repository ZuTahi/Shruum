using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider; // assign in prefab
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    private Transform target;
    private Vector3 baseOffset;
    private Vector3 offset;
    private Quaternion fixedRotation;
    private Coroutine shakeRoutine;

    // Call this after Instantiate
    public void AttachTo(Transform targetTransform, Vector3 offsetPosition)
    {
        target = targetTransform;
        baseOffset = offsetPosition;
        offset = offsetPosition;

        // Lock the bar to the camera's current facing (does NOT keep tracking camera rotation)
        if (Camera.main != null)
            fixedRotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        else
            fixedRotation = Quaternion.identity;

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void SetMaxHealth(float maxHealth)
    {
        if (healthSlider == null) return;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    public void SetHealth(float currentHealth)
    {
        if (healthSlider == null) return;
        healthSlider.value = currentHealth;

        // 🟢 trigger shake when taking damage
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // small random jitter around base offset
            offset = baseOffset + (Vector3)Random.insideUnitCircle * shakeMagnitude;

            yield return null;
        }

        // reset offset
        offset = baseOffset;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = fixedRotation; // stays fixed (doesn't spin with enemy/camera)
    }
}
