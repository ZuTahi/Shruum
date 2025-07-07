using UnityEngine;

public class VFXAutoDestroy : MonoBehaviour
{
    private ParticleSystem ps;
    void Start() => ps = GetComponent<ParticleSystem>();

    void Update()
    {
        if (ps && !ps.IsAlive())
            Destroy(gameObject);
    }
}
