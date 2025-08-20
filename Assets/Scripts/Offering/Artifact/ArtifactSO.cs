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
        if (RunData.CurrentRun == null)
        {
            Debug.LogWarning("Cannot apply artifact: No active run!");
            return;
        }

        RunData.CurrentRun.AddArtifact(this);
    }
}
