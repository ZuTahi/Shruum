using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public int natureForce = 0;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public void AddNatureForce(int amount)
    {
        natureForce += amount;
        Debug.Log("Nature Force: " + natureForce);
        // Update UI if needed
    }

}
