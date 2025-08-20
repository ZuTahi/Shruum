using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DropGroundSettler : MonoBehaviour
{
    public float settleDelay = 0.4f;   // time before freezing
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // make sure gravity is on so it falls
        rb.useGravity = true;
        rb.isKinematic = false;

        Invoke(nameof(Freeze), settleDelay);
    }

    void Freeze()
    {
        // freeze in place after it had time to bounce
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
    }
}
