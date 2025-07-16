using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public int currentHP;
    public int currentSP;
    public int currentMP;

    public float attackDamage;
    public float baseDefensePercent;
    public float defenseMultiplier;

    public int natureForce;
    public List<string> loreNotes;

    public PlayerSaveData(PlayerStats stats, PlayerInventory inventory)
    {
        currentHP = stats.currentHP;
        currentSP = stats.currentSP;
        currentMP = stats.currentMP;

        attackDamage = stats.attackDamage;
        baseDefensePercent = stats.baseDefensePercent;
        defenseMultiplier = stats.defenseMultiplier;

        natureForce = inventory.natureForce;
        loreNotes = new List<string>(inventory.loreNotes);
    }
}
