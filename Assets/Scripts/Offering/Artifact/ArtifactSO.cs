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
        var playerStats = PlayerStats.Instance;
        if (playerStats == null)
        {
            Debug.LogWarning("Cannot apply artifact effect: PlayerStats not found.");
            return;
        }

        Debug.Log($"Applying effect of artifact: {artifactName}");

        switch (effectType)
        {
            case ArtifactType.IncreaseHP:
                playerStats.IncreaseMaxHP(effectAmount);
                Debug.Log($"Increased Max HP by {effectAmount}");
                break;

            case ArtifactType.IncreaseSP:
                playerStats.IncreaseMaxSP(effectAmount);
                Debug.Log($"Increased Max SP by {effectAmount}");
                break;

            case ArtifactType.IncreaseMP:
                playerStats.IncreaseMaxMP(effectAmount);
                Debug.Log($"Increased Max MP by {effectAmount}");
                break;

            case ArtifactType.IncreaseAttack:
                playerStats.attackMultiplier += effectAmount;
                Debug.Log($"Increased Attack Multiplier by {effectAmount}");
                break;

            case ArtifactType.IncreaseDefense:
                playerStats.defenseMultiplier += effectAmount;
                Debug.Log($"Increased Defense Multiplier by {effectAmount}");
                break;
        }

        playerStats.RefreshUI();
    }
}
