using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using EditorAttributes;
using EasyTransition;

public class MainMenu : MenuPage
{
    [Header("Main Menu")]
    [SerializeField, SceneDropdown] private int _gameScene = 1;
    [SerializeField, Required] private TransitionSettings _transitionSettings;
    [SerializeField] private float _cameraSwayFrequency = 1f;
    [SerializeField] private float _cameraSwayAmplitude = 1.5f;

    private Camera2D _camera;

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(Camera2D.Current, $"[{name}] {nameof(Camera2D)} instance not found in scene");
        _camera = Camera2D.Current;
    }

    protected override void Start()
    {
        base.Start();

        // Update version label (usually at the bottom right)
        Container.Q<Button>("PlayButton").clicked += OnPlayButtonClicked;
        Container.Q<Button>("QuitButton").clicked += OnQuitButtonClicked;
        Container.Q<Label>("VersionLabel").text = $"v{Application.version}";

        // Focus on the first button
        TryFocus();

        // Camera sway
        _camera.AddPersistentShake(_cameraSwayFrequency, _cameraSwayAmplitude);
    }

    private void OnPlayButtonClicked()
    {
        TransitionManager.Instance().Transition(_gameScene, _transitionSettings, 0);
    }

    private void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private static void Log(object message, Object ctx) => Debug.Log($"[{nameof(MainMenu)}] {message}", ctx);
}
