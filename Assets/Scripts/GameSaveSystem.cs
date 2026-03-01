using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int currentStep;
    public string currentRoomId;

    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;

    public List<string> inventoryItemIds = new List<string>();
}

public static class GameSaveSystem
{
    private const string SaveKey = "GAME_SAVE_SLOT_0";

    public static void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("[SaveSystem] Saved: " + json);
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public static GameSaveData Load()
    {
        if (!HasSave())
            return null;

        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveSystem] Load failed: " + e.Message);
            return null;
        }
    }

    public static void Delete()
    {
        if (PlayerPrefs.HasKey(SaveKey))
            PlayerPrefs.DeleteKey(SaveKey);

        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Save deleted.");
    }
}
