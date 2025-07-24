using UnityEngine;

public class NatureForceDrop : MonoBehaviour
{
    public int value = 1;
    public float magnetRadius = 2.5f;
    public float pullSpeed = 5f;
    public float collectDistance = 0.3f;
    public float pickupDelay = 0.3f;  // prevent instant pickup

    private Transform player;
    private bool isPickupEnabled = false;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        Invoke(nameof(EnablePickup), pickupDelay);
    }

    private void EnablePickup()
    {
        isPickupEnabled = true;
    }

    private void Update()
    {
        if (player == null || !isPickupEnabled) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < magnetRadius)
        {
            Vector3 targetPos = player.position;
            transform.position = Vector3.Lerp(transform.position, targetPos, pullSpeed * Time.deltaTime);
        }

        if (distance < collectDistance)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddNatureForce(value);
                Debug.Log($"Collected Nature Force: {value}");
                Destroy(gameObject);
            }
        }
    }
}
