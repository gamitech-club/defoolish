using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class SavedGame
{
    private const bool IsEncrypted = true;
    private const string Password = "plkzrhtrpq";
    private const int CurrentVersion = 1;

    public static readonly string FilePath = Path.Combine(Application.persistentDataPath, "SaveGame");
    public static SavedGame Instance = LoadData();

    #region Serialized fields
    public int Version = CurrentVersion;
    public HashSet<EndingID> UnlockedEndings = new();
    #endregion

    /// <summary>
    /// Saves game data to file.
    /// </summary>
    public void Save()
    {
        Log($"Saving game..");

        string content = JsonConvert.SerializeObject(this, Formatting.Indented);
        if (IsEncrypted) {
            content = EncryptDecrypt(content);
        }

        File.WriteAllText(FilePath, content);
    }

    /// <summary>
    /// Loads saved game data from file. Returns default values if file doesn't exist.
    /// </summary>
    private static SavedGame LoadData()
    {
        if (!File.Exists(FilePath))
        {
            Log($"Save file not found at '{FilePath}'. Using default values.");
            return new SavedGame();
        }

        Log($"Loading player saved game..");

        var savedGame = new SavedGame();
        string content = File.ReadAllText(FilePath);

        if (IsEncrypted)
            content = EncryptDecrypt(content);

        try
        {
            savedGame = JsonConvert.DeserializeObject<SavedGame>(content);
            if (savedGame.Version < CurrentVersion)
            {
                LogWarning($"Your save file version is outdated. Updating to v{CurrentVersion}. Errors might occur");
                savedGame.Version = CurrentVersion;
                savedGame.Save();
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to load save data: {e.Message}");
        }

        return savedGame;
    }

    private static string EncryptDecrypt(string content)
    {
        string modified = "";
        for (int i = 0; i < content.Length; i++)
            modified += (char)(content[i] ^ Password[i % Password.Length]);
        return modified;
    }

    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Save System/Print Save File (Disk)")]
    private static void EditorPrintDataOnDisk()
    {
        var data = LoadData();
        if (data) {
            Debug.Log(JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }

    [UnityEditor.MenuItem("Tools/Save System/Print Save File (Memory)")]
    private static void EditorPrintDataOnMemory()
    {
        var data = Instance;
        if (data) {
            Debug.Log(JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
    #endif

    public static implicit operator bool(SavedGame data) => data != null;
    private static void Log(object message) => Debug.Log($"[{nameof(SavedGame)}] {message}");
    private static void LogWarning(object message) => Debug.LogWarning($"[{nameof(SavedGame)}] {message}");
    private static void LogError(object message) => Debug.LogError($"[{nameof(SavedGame)}] {message}");
}
