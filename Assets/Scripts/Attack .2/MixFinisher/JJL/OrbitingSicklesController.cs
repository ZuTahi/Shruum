using UnityEngine;

public class OrbitingSicklesController : MonoBehaviour
{
    public Transform followTarget;
    public float rotationSpeed = 180f;
    public float lifetime = 3f;

    private float timer;

    void Start()
    {
        timer = lifetime;
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
}
