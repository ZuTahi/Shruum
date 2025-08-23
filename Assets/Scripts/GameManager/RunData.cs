// --- RunData.cs ---
using System.Collections.Generic;
using UnityEngine;

public class RunData
{
    public static int bonusHP = 0;
    public static int bonusSP = 0;
    public static int bonusMP = 0;
    public static float bonusAttack = 0f;
    public static float bonusDefense = 0f;

    // Singleton-like instance for current run
    public static RunData CurrentRun { get; private set; }

    // List of temporary artifacts picked up this run
    public List<ArtifactSO> acquiredArtifacts = new List<ArtifactSO>();

    // Optional: track run start time, room progress, gold, etc.
    public int roomsCleared = 0;

    // Initialize a new run
    public static void StartNewRun()
    {
        CurrentRun = new RunData();
        Debug.Log("[RunData] New run started.");
    }

    // Add an artifact (boon-like buff)
    public void AddArtifact(ArtifactSO artifact)
    {
        acquiredArtifacts.Add(artifact);
        ApplyArtifactEffect(artifact);

        Debug.Log($"[RunData] Acquired artifact: {artifact.artifactName}");
    }

    // Apply the artifact's effect immediately to PlayerStats
    private void ApplyArtifactEffect(ArtifactSO artifact)
    {
        var playerStats = Object.FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("[RunData] No PlayerStats found to apply artifact.");
            return;
        }
        // ✅ Capture ratios before buffs are applied
        float hpRatio = (playerStats.maxHP > 0) ? (float)playerStats.currentHP / playerStats.maxHP : 1f;
        bool hpWasFull = playerStats.currentHP >= playerStats.maxHP;

        float mpRatio = (playerStats.maxMP > 0) ? (float)playerStats.currentMP / playerStats.maxMP : 1f;
        bool mpWasFull = playerStats.currentMP >= playerStats.maxMP;

        switch (artifact.effectType)
        {
            case ArtifactSO.ArtifactType.IncreaseHP:
                bonusHP += artifact.effectAmount;
                break;
            case ArtifactSO.ArtifactType.IncreaseSP:
                bonusSP += artifact.effectAmount;
                break;
            case ArtifactSO.ArtifactType.IncreaseMP:
                bonusMP += artifact.effectAmount;
                break;
            case ArtifactSO.ArtifactType.IncreaseAttack:
                bonusAttack += artifact.effectAmount;
                break;
            case ArtifactSO.ArtifactType.IncreaseDefense:
                bonusDefense += artifact.effectAmount;
                break;
        }

        playerStats.LoadFromData();
        playerStats.currentHP = hpWasFull ? playerStats.maxHP : Mathf.RoundToInt(hpRatio * playerStats.maxHP);
        playerStats.currentMP = mpWasFull ? playerStats.maxMP : Mathf.RoundToInt(mpRatio * playerStats.maxMP);
        PlayerData.currentHP = playerStats.currentHP;
        PlayerData.currentSP = playerStats.currentSP;
        PlayerData.currentMP = playerStats.currentMP;
        Debug.Log($"[RunData] Artifact applied and buffs updated: {artifact.artifactName}");
        playerStats.RefreshUI();
    }

    public static void ApplyRunBuffs(PlayerStats stats)
    {
        stats.maxHP += bonusHP;
        stats.maxSP += bonusSP;
        stats.maxMP += bonusMP;
        stats.attackMultiplier += bonusAttack;
        stats.defenseMultiplier += bonusDefense;

        Debug.Log($"[RunData] Buffs applied: +{bonusHP} HP, +{bonusSP} SP, etc.");
    }

    // Clear run data after death, quit, or victory
    public static void ClearRunData()
    {
        bonusHP = 0;
        bonusSP = 0;
        bonusMP = 0;
        bonusAttack = 0f;
        bonusDefense = 0f;

        if (CurrentRun != null)
        {
            CurrentRun.acquiredArtifacts.Clear();
            CurrentRun.roomsCleared = 0;
            CurrentRun = null;
        }

        Debug.Log("[RunData] Run data cleared (bonuses reset).");
    }
}
