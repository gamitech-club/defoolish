using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityEngine.Assertions;
using EditorAttributes;

using Button = UnityEngine.UIElements.Button;
using Slider = UnityEngine.UIElements.Slider;

public class SettingsMenu : MenuPage
{
    [Header("Settings Menu")]
    [SerializeField, Required] private AudioMixer _audioMixer;
    [SerializeField] private bool _isInPauseMenu;

    private bool _isEventsRegistered;

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(_audioMixer, $"[{name}] AudioMixer not assigned");

        if (_isInPauseMenu)
            Container.AddToClassList("in-pause-menu");
    }

    protected override void Start()
    {
        base.Start();
        SetupSettings();
    }

    private void SetupSettings()
    {
        var settings = SavedSettings.Instance;

        // Setup sliders
        SetupSlider("MasterVolumeSlider", settings.MasterVolume, OnMasterVolumeSliderChanged);
        SetupSlider("MusicVolumeSlider", settings.MusicVolume, OnMusicVolumeSliderChanged);
        SetupSlider("SFXVolumeSlider", settings.SFXVolume, OnSFXVolumeSliderChanged);
        SetupSlider("VoiceVolumeSlider", settings.VoiceVolume, OnVoiceVolumeSliderChanged);
        if (!_isEventsRegistered)
            Container.Q<Button>("ResetButton").clicked += OnResetButtonClicked;

        _isEventsRegistered = true;

        // Apply settings
        SetAudioVolume("MasterVolume", settings.MasterVolume);
        SetAudioVolume("MusicVolume", settings.MusicVolume);
        SetAudioVolume("SFXVolume", settings.SFXVolume);
        SetAudioVolume("VoiceVolume", settings.VoiceVolume);
    }

    private Slider SetupSlider(string sliderName, float initialValue, Action<float> onValueChanged)
    {
        Slider slider = Container.Q<Slider>(sliderName);
        Assert.IsNotNull(slider, $"[{name}] Slider named '{sliderName}' not found.");

        slider.SetValueWithoutNotify(initialValue);
        if (!_isEventsRegistered)
            slider.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));

        return slider;
    }

    private void SetAudioVolume(string audio, float normalizedVolume)
    {
        if (!_audioMixer.SetFloat(audio, Mathf.Lerp(-80f, 20f, normalizedVolume)))
            Debug.LogError($"Failed to set audio '{audio}'", this);
    }

    private void DelayedSave(float delay = .2f)
    {
        CancelInvoke(nameof(Save));
        Invoke(nameof(Save), delay);
    }

    private void Save()
        => SavedSettings.Instance.Save();

    private void OnMasterVolumeSliderChanged(float value)
    {
        SetAudioVolume("MasterVolume", value);
        SavedSettings.Instance.MasterVolume = value;
        DelayedSave();
    }

    private void OnMusicVolumeSliderChanged(float value)
    {
        SetAudioVolume("MusicVolume", value);
        SavedSettings.Instance.MusicVolume = value;
        DelayedSave();
    }

    private void OnSFXVolumeSliderChanged(float value)
    {
        SetAudioVolume("SFXVolume", value);
        SavedSettings.Instance.SFXVolume = value;
        DelayedSave();
    }

    private void OnVoiceVolumeSliderChanged(float value)
    {
        SetAudioVolume("VoiceVolume", value);
        SavedSettings.Instance.VoiceVolume = value;
        DelayedSave();
    }

    private void OnResetButtonClicked()
    {
        SavedSettings.DeleteFile();
        SavedSettings.ResetInstance();
        SetupSettings();
    }

    #if UNITY_EDITOR
    [Button("Print Settings")]
    private void EditorPrintSettings()
    {
        if (!File.Exists(SavedSettings.FilePath))
        {
            Debug.LogError($"Settings file not found");
            return;
        }

        try
        {
            var jsonString = File.ReadAllText(SavedSettings.FilePath);
            var settingsObj = Newtonsoft.Json.JsonConvert.DeserializeObject<SavedSettings>(jsonString);
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(settingsObj, Newtonsoft.Json.Formatting.Indented));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to print settings: {e.Message}\n{e}");
        }
        
    }
    #endif
}
