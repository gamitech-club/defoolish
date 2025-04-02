using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using EasyTransition;
using EditorAttributes;

public class PauseMenu : MenuPage
{
    #region Singleton
    private static PauseMenu _instance;
    public static PauseMenu Instance {
        get {
            if (_instance == null)
                _instance = FindFirstObjectByType<PauseMenu>();
            return _instance;
        }
    }
    #endregion
    
    [Header("Pause Menu")]
    [SerializeField, SceneDropdown] private int _mainMenuScene;
    [SerializeField] private TransitionSettings _transitionRestart;
    [SerializeField] private TransitionSettings _transitionMainMenu;
    [SerializeField, Tooltip("The menus that should hide when the pause menu is hidden")] private MenuPage[] _subMenus;

    public event Action PauseStateChanged;
    public bool IsPaused => _isPaused;

    private InputAction _pauseAction;
    private bool _isPaused;

    protected override void Awake()
    {
        base.Awake();

        // If an instance already exists, destroy the new one
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {nameof(PauseMenu)} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _pauseAction = InputSystem.actions.FindAction(ActionNames.Cancel);
    }

    protected override void Start()
    {
        base.Start();

        Container.Q<Button>("ResumeButton").clicked += OnResumeButtonClicked;;
        Container.Q<Button>("RestartButton").clicked += OnRestartLevelButtonClicked;
        Container.Q<Button>("MainMenuButton").clicked += OnMainMenuButtonClicked;
    }

    private void OnEnable()
    {
        _pauseAction.performed += OnPauseActionPerformed;
    }

    private void OnDisable()
    {
        _pauseAction.performed -= OnPauseActionPerformed;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        _isPaused = true;
        Show();
        TryFocus();

        PauseStateChanged?.Invoke();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        Hide();

        // Hide menus
        foreach (var menu in _subMenus)
            if (!menu.IsHidden)
                menu.Hide();

        PauseStateChanged?.Invoke();
    }

    private void OnPauseActionPerformed(InputAction.CallbackContext ctx)
    {
        bool canPause =
            !TransitionManager.Instance().IsTransitioning &&
            GameOverMenu.Instance.IsHidden;

        if (!canPause)
            return;

        if (_isPaused) Resume();
        else Pause();
    }

    private void OnResumeButtonClicked()
    {
        Resume();
    }

    private void OnRestartLevelButtonClicked()
    {
        Resume();

        var currentScene = SceneManager.GetActiveScene().buildIndex;
        TransitionManager.Instance().Transition(currentScene, _transitionRestart, 0f);
    }

    private void OnMainMenuButtonClicked()
    {
        Resume();
        TransitionManager.Instance().Transition(_mainMenuScene, _transitionMainMenu, 0f);
    }
}
