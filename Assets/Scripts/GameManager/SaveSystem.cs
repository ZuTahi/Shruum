// --- SaveSystem.cs (Refactored) ---
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string savePath => Application.persistentDataPath + "/playerSave.shrum";

    public static void SavePlayer()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        PlayerSaveData data = new PlayerSaveData();

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Game Saved.");
    }

    public static void LoadPlayer()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);

            PlayerSaveData data = formatter.Deserialize(stream) as PlayerSaveData;
            stream.Close();

            if (data != null)
            {
                PlayerData.LoadFromSaveData(data);
                Debug.Log("Game Loaded.");
            }
        }
        else
        {
            Debug.LogWarning("Save file not found.");
        }
    }

    // ✅ New Method to clear save data
    public static void ClearSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file cleared.");
        }
        else
        {
            Debug.Log("No save file to clear.");
        }
    }

}


[System.Serializable]
public class PlayerSaveData
{
    public int natureForce;
    public int maxHP;
    public int maxSP;
    public int maxMP;
    public float attackMultiplier;
    public float baseDefensePercent;
    public float defenseMultiplier;
    public string[] loreNotes;

    public WeaponType[] unlockedWeapons;   // ✅ Add this

    public PlayerSaveData()
    {
        natureForce = PlayerData.natureForce;
        maxHP = PlayerData.maxHP;
        maxSP = PlayerData.maxSP;
        maxMP = PlayerData.maxMP;
        attackMultiplier = PlayerData.attackMultiplier;
        baseDefensePercent = PlayerData.baseDefensePercent;
        defenseMultiplier = PlayerData.defenseMultiplier;
        loreNotes = PlayerData.loreNotes.ToArray();

        unlockedWeapons = new WeaponType[PlayerData.unlockedWeapons.Count];
        PlayerData.unlockedWeapons.CopyTo(unlockedWeapons);
    }
}

