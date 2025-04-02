using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using EditorAttributes;
using EasyTransition;

public class GameOverMenu : MenuPage
{
    #region Singleton
    private static GameOverMenu _instance = null;
    public static GameOverMenu Instance {
        get {
            if (_instance == null)
                _instance = FindFirstObjectByType<GameOverMenu>(FindObjectsInactive.Include);
            return _instance;
        }
    }
    #endregion

    [SerializeField, Required] private TransitionSettings _restartTransition;
    [SerializeField, Required] private TransitionSettings _mainMenuTransition;
    [SerializeField, SceneDropdown] private int _mainMenuScene;

    private Label _titleLabel;
    private Label _subtitleLabel;
    private Label _detailsLabel;

    protected override void Awake()
    {
        base.Awake();

        // If an instance already exists, destroy the new one
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {nameof(GameOverMenu)} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        _titleLabel = Container.Q<Label>("TitleLabel");
        _subtitleLabel = Container.Q<Label>("SubtitleLabel");
        _detailsLabel = Container.Q<Label>("DetailsLabel");
    }

    protected override void Start()
    {
        base.Start();
        Container.Q<Button>("RestartButton").clicked += OnRestartButtonClicked;
        Container.Q<Button>("MainMenuButton").clicked += OnMainMenuButtonClicked;
    }

    public void Setup(string title, string subtitle, string details)
    {
        _titleLabel.text = title;
        _subtitleLabel.text = subtitle;
        _detailsLabel.text = details;
    }

    private void OnRestartButtonClicked()
    {
        var scene = SceneManager.GetActiveScene().buildIndex;
        TransitionManager.Instance().Transition(scene, _restartTransition, 0f);
        Time.timeScale = 1f;
    }

    private void OnMainMenuButtonClicked()
    {
        TransitionManager.Instance().Transition(_mainMenuScene, _mainMenuTransition, 0f);
        Time.timeScale = 1f;
    }
}
