using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string savePath => Application.persistentDataPath + "/player.sav";

    public static void SavePlayer(PlayerStats stats, PlayerInventory inventory)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        PlayerSaveData data = new PlayerSaveData(stats, inventory);
        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Game Saved to: " + savePath);
    }

    public static void LoadPlayer(PlayerStats stats, PlayerInventory inventory)
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);

            PlayerSaveData data = formatter.Deserialize(stream) as PlayerSaveData;
            stream.Close();

            if (data != null)
            {
                stats.LoadData(data);
                inventory.LoadData(data);
                Debug.Log("Game Loaded from: " + savePath);
            }
        }
        else
        {
            Debug.LogWarning("No save file found at: " + savePath);
        }
    }
}
