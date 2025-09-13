using UnityEngine;

public class OrbitingSicklesController : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform followTarget;
    public float rotationSpeed = 180f;
    public float lifetime = 3f;

    private float timer;

    [Header("Audio")]
    public AudioClip loopClip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;         // 🔁 continuous loop
        audioSource.spatialBlend = 0.7f; // positional sound
    }

    void Start()
    {
        timer = lifetime;

        // start looping audio
        if (loopClip != null)
        {
            audioSource.clip = loopClip;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (followTarget != null)
            transform.position = followTarget.position;

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
