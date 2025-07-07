using UnityEngine;

[System.Serializable]
public class OfferingData
{
    public StatType statType;
    public int amount = 0;
    public MeshRenderer[] visualStages; // Optional: fills in visuals on the shrine
}
