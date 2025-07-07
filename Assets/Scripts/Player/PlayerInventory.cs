using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public int natureForce = 0;
    public int[] artifactCounts = new int[5]; // 0 = HP, etc.

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public bool HasArtifact(StatType type) => artifactCounts[(int)type] > 0;

    public void SpendArtifact(StatType type)
    {
        int i = (int)type;
        if (artifactCounts[i] > 0)
            artifactCounts[i]--;
    }

    public void AddArtifact(StatType type)
    {
        artifactCounts[(int)type]++;
    }

    public void AddNatureForce(int amount)
    {
        natureForce += amount;
    }
}
