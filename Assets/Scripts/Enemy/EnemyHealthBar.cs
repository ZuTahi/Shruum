using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider; // assign in prefab

    private Transform target;
    private Vector3 offset;
    private Quaternion fixedRotation;

    // Call this after Instantiate
    public void AttachTo(Transform targetTransform, Vector3 offsetPosition)
    {
        target = targetTransform;
        offset = offsetPosition;

        // Lock the bar to the camera's current facing (does NOT keep tracking camera rotation)
        if (Camera.main != null)
            fixedRotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        else
            fixedRotation = Quaternion.identity;
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
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = fixedRotation; // stays fixed (doesn't spin with enemy/camera)
    }
}
