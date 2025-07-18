using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact", menuName = "Artifacts/Artifact")]
public class ArtifactSO : ScriptableObject
{
    public string artifactName;
    public string description;
    public Sprite artifactIcon;

    public enum ArtifactType { IncreaseHP, IncreaseSP, IncreaseMP, IncreaseAttack, IncreaseDefense }
    public ArtifactType effectType;
    public int effectAmount;

    public void ApplyEffect()
    {
        Debug.Log("Applying effect of artifact: " + artifactName);

        switch (effectType)
        {
            case ArtifactType.IncreaseHP:
                PlayerStats.Instance.maxHP += effectAmount;
                Debug.Log("Increased Max HP by " + effectAmount);
                break;

            case ArtifactType.IncreaseSP:
                PlayerStats.Instance.maxSP += effectAmount;
                Debug.Log("Increased Max SP by " + effectAmount);
                break;

            case ArtifactType.IncreaseMP:
                PlayerStats.Instance.attackMultiplier += effectAmount;
                Debug.Log("Increased Max MP by " + effectAmount);
                break;
            case ArtifactType.IncreaseAttack:
                PlayerStats.Instance.attackMultiplier += effectAmount;
                Debug.Log("Increased Attack Multiplier by " + effectAmount);
                break;
            case ArtifactType.IncreaseDefense:
                PlayerStats.Instance.attackMultiplier += effectAmount;
                Debug.Log("Increased Defense Multiplier by " + effectAmount);
                break;
        }
    }
}
