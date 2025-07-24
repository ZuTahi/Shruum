using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop Prefabs")]
    public GameObject manaDropPrefab;
    public GameObject natureForceDropPrefab;

    [Header("Drop Values")]
    public int manaAmount = 1;
    public int natureForceAmount = 1;
    public float dropSpreadRadius = 0.5f;
    public float dropUpwardForce = 2f;  // new: for small bounce

    public void SetDropValues(int mana, int natureForce)
    {
        manaAmount = mana;
        natureForceAmount = natureForce;
    }

    public void DropItems()
    {
        for (int i = 0; i < manaAmount; i++)
        {
            Vector3 offset = Random.insideUnitCircle * dropSpreadRadius;
            GameObject manaObj = Instantiate(manaDropPrefab, transform.position + offset, Quaternion.identity);

            if (manaObj.TryGetComponent<ManaDrop>(out var manaDrop))
            {
                manaDrop.manaValue = Random.Range(1, 4);
            }
        }

        for (int i = 0; i < natureForceAmount; i++)
        {
            Vector3 offset = Random.insideUnitCircle * dropSpreadRadius;
            GameObject nfObj = Instantiate(natureForceDropPrefab, transform.position + offset, Quaternion.identity);

            if (nfObj.TryGetComponent<NatureForceDrop>(out var nfDrop))
            {
                nfDrop.value = Random.Range(5, 20);
            }
        }
    }

    private void DropPrefab(GameObject prefab, int amount)
    {
        if (prefab == null || amount <= 0) return;

        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * dropSpreadRadius;
            offset.y = 0f;  // stay on ground plane

            GameObject drop = Instantiate(prefab, transform.position + offset, Quaternion.identity);

            if (drop.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 randomForce = Vector3.up * dropUpwardForce + Random.insideUnitSphere;
                rb.AddForce(randomForce, ForceMode.Impulse);
            }
        }
    }
}
