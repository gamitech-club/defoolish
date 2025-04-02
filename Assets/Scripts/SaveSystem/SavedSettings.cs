using System.IO;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class SavedSettings
{
    private const int CurrentVersion = 2;
    public static readonly string FilePath = Path.Combine(Application.persistentDataPath, "Settings.json");
    public static SavedSettings Instance = LoadInstance();

    #region Serialized fields
    public int Version = CurrentVersion;
    public float MasterVolume = .8f;
    public float MusicVolume = .8f;
    public float SFXVolume = .8f;
    public float VoiceVolume = .8f;
    #endregion

    private static readonly JsonSerializerSettings JsonSettings = new() {
        ContractResolver = new FieldsOnlyContractResolver()
    };

    /// <summary>
    /// Saves settings to file.
    /// </summary>
    public void Save()
    {
        Log($"Saving player settings..");

        var jsonText = JsonConvert.SerializeObject(this, Formatting.Indented, JsonSettings);
        File.WriteAllText(FilePath, jsonText);
    }

    /// <summary>
    /// Loads settings from file. Returns default values if file doesn't exist.
    /// </summary>
    private static SavedSettings LoadInstance()
    {
        if (!File.Exists(FilePath))
        {
            Log($"Settings file not found at '{FilePath}'. Using default values.");
            return new SavedSettings();
        }

        Log($"Loading player settings..");

        var settings = new SavedSettings();
        var jsonString = File.ReadAllText(FilePath);

        try
        {
            settings = JsonConvert.DeserializeObject<SavedSettings>(jsonString, JsonSettings);
            if (settings.Version < CurrentVersion)
            {
                LogWarning($"Your settings file version is outdated. Resetting settings.");
                settings = new SavedSettings();
                settings.Save();
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to load settings: {e.Message}\n{e}");
        }

        return settings;
    }

    public static void ResetInstance()
        => Instance = new SavedSettings();

    public static void DeleteFile()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }

    private static void Log(object message) => Debug.Log($"[{nameof(SavedSettings)}] {message}");
    private static void LogWarning(object message) => Debug.LogWarning($"[{nameof(SavedSettings)}] {message}");
    private static void LogError(object message) => Debug.LogError($"[{nameof(SavedSettings)}] {message}");
}

/// <summary>
/// Fields only contract resolver. Use to prevent properties like
/// normalized, magnitude, etc from being serialized.
/// </summary>
public class FieldsOnlyContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        // Check member types
        if (member.MemberType == MemberTypes.Property)
            property.ShouldSerialize = _ => false; // Ignore all properties

        return property;
    }
}
