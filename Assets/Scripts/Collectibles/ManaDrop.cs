using UnityEngine;

public class ManaDrop : MonoBehaviour
{
    public int manaValue = 1;
    public float magnetRadius = 2.5f;
    public float pullSpeed = 5f;
    public float collectDistance = 0.3f;
    public float pickupDelay = 0.3f;

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
            transform.position = Vector3.Lerp(transform.position, player.position, pullSpeed * Time.deltaTime);
        }

        if (distance < collectDistance)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddMana(manaValue);
                Debug.Log($"Collected Mana: {manaValue}");
                Destroy(gameObject);
            }
        }
    }
}
