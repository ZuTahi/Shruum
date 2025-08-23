using System;
using System.Reflection;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Coroutine shakeRoutine;

    // Cinemachine reflection objects (only used if Cinemachine is present)
    private Component cmVirtualCamera;
    private Component cmNoise;
    private FieldInfo cmAmplitudeField;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Try to find Cinemachine types at runtime (no compile-time dependency)
        TryFindCinemachineComponents();
    }

    private void TryFindCinemachineComponents()
    {
        try
        {
            Type vcamType = null;
            Type noiseType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (vcamType == null) vcamType = asm.GetType("Cinemachine.CinemachineVirtualCamera");
                if (noiseType == null) noiseType = asm.GetType("Cinemachine.CinemachineBasicMultiChannelPerlin");
                if (vcamType != null && noiseType != null) break;
            }

            if (vcamType == null || noiseType == null) return;

            // if this GameObject has a Cinemachine Virtual Camera, use it
            cmVirtualCamera = GetComponent(vcamType);
            if (cmVirtualCamera == null) return;

            // CinemachineVirtualCamera has a method GetCinemachineComponent<T>()
            MethodInfo getComp = vcamType.GetMethod("GetCinemachineComponent", BindingFlags.Instance | BindingFlags.Public);
            if (getComp == null) return;

            // call GetCinemachineComponent(typeof(CinemachineBasicMultiChannelPerlin))
            cmNoise = (Component)getComp.Invoke(cmVirtualCamera, new object[] { noiseType });
            if (cmNoise == null) return;

            // the noise component exposes a (serialised) field m_AmplitudeGain
            cmAmplitudeField = noiseType.GetField("m_AmplitudeGain", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // if cmAmplitudeField is null, we still won't fail — fallback will be used
        }
        catch
        {
            // reflection can fail if Cinemachine isn't present - ignore and fallback to transform shake
        }
    }

    /// <summary>
    /// Shake entrypoint used by other scripts.
    /// If Cinemachine noise is available it will use that; otherwise falls back to transform-local shake.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        // stop any existing shake
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        // Use Cinemachine noise if available and has the amplitude field
        if (cmNoise != null && cmAmplitudeField != null)
        {
            shakeRoutine = StartCoroutine(ShakeCinemachineRoutine(duration, magnitude));
        }
        else
        {
            shakeRoutine = StartCoroutine(ShakeTransformRoutine(duration, magnitude));
        }
    }

    // Cinemachine-based shake (uses reflection to modify amplitude without compile-time dependency)
    private IEnumerator ShakeCinemachineRoutine(float duration, float magnitude)
    {
        // read original amplitude (if possible)
        float originalAmp = 0f;
        var val = cmAmplitudeField.GetValue(cmNoise);
        if (val is float f) originalAmp = f;

        // set amplitude
        cmAmplitudeField.SetValue(cmNoise, magnitude);

        yield return new WaitForSeconds(duration);

        // restore
        cmAmplitudeField.SetValue(cmNoise, originalAmp);
        shakeRoutine = null;
    }

    // Transform-local shake (safe fallback)
    private IEnumerator ShakeTransformRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        // Capture the base position at the *start* of the shake so we offset from current
        Vector3 baseLocalPos = transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = baseLocalPos + new Vector3(x, y, 0f);

            yield return null;
        }

        // restore to the captured base
        transform.localPosition = baseLocalPos;
        shakeRoutine = null;
    }
}
