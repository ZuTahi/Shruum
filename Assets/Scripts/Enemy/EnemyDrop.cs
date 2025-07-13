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
            Instantiate(manaDropPrefab, transform.position + offset, Quaternion.identity);
        }

        for (int i = 0; i < natureForceAmount; i++)
        {
            Vector3 offset = Random.insideUnitCircle * dropSpreadRadius;
            Instantiate(natureForceDropPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}
