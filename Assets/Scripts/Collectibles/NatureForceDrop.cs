using UnityEngine;

public class NatureForceDrop : MonoBehaviour
{
    public int value = 1;
    public float magnetRadius = 2.5f;
    public float pullSpeed = 5f;
    public float collectDistance = 0.3f;

    private Transform player;

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null)
                player = found.transform;
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance < magnetRadius)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * pullSpeed * Time.deltaTime;
            }

            if (distance < collectDistance)
            {
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.AddNatureForce(value);
                    Debug.Log("Collected Nature Force: " + value);
                    Destroy(gameObject);
                }
            }
        }
    }
}
