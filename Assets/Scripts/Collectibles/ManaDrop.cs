using UnityEngine;

public class ManaDrop : MonoBehaviour
{
    public int manaValue = 1;
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
                // Move towards player
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * pullSpeed * Time.deltaTime;
            }

            // Collect if close enough
            if (distance < collectDistance)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.AddMana(manaValue);
                    Debug.Log("Collected Mana: " + manaValue);
                    Destroy(gameObject);
                }
            }
        }
    }
}
