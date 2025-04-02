using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

[Serializable]
public class DialogueLine
{
    [TextArea]
    public string Line = "Hello World";
    public AudioResource Audio;
    public float SubtitleStartDelay;
    [Tooltip("Delay addition before this line ends.")] public float DurationAddition = .3f;
    [Tooltip("Optional, used for storyline & events.")] public string Name;
    [Range(0, 2f)] public float VolumeMultiplier = 1f;
    public bool WriteInstantly;
}

public class DialogueManager : MonoBehaviour
{
    #region Singleton
    private static DialogueManager _instance;
    public static DialogueManager Instance {
        get {
            if (_instance == null)
                _instance = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
            return _instance;
        }
    }
    #endregion

    [SerializeField, Required] private TypewriterEffect _typewriter;
    [SerializeField, Required] private TextMeshProUGUI _subtitleLabel;
    [SerializeField, Required] private Image _background;
    [SerializeField, Required] private AudioSource _audioSource;

    [Header("Test Dialogue")]
    [SerializeField] private DialogueLine[] _testDialogue;
    [SerializeField] private bool _useTestDialogue;

    public event EventHandler<LineEventArgs> LineStarted;
    public event EventHandler<LineEventArgs> LineCompleted;
    public class LineEventArgs : EventArgs { public DialogueLine Line; }

    public bool IsInDialogue => _isInDialogue;

    private const float BackgroundFadeDuration = .5f;

    private float _bgDefaultAlpha;
    private Tween _bgFadeTween;
    private CancellationTokenSource _dialogueCancellation;
    private bool _isInDialogue;
    private float _defaultVolume;

    private void Reset()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {nameof(DialogueManager)} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        _bgDefaultAlpha = _background.color.a;
        _bgFadeTween?.Kill();
        _bgFadeTween = _background.DOFade(0, 0);
        _subtitleLabel.text = string.Empty;
        _defaultVolume = _audioSource.volume;
    }

    private void Start()
    {
        if (_useTestDialogue)
            TestDialogue().Forget();
    }

    private void OnEnable()
    {
        if (PauseMenu.Instance)
            PauseMenu.Instance.PauseStateChanged += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        if (PauseMenu.Instance)
            PauseMenu.Instance.PauseStateChanged -= OnPauseStateChanged;
    }

    private void OnDestroy()
    {
        _dialogueCancellation?.Cancel();
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
        {
            Debug.Log("[TEST] Cancelling dialogue..");
            CancelDialogue();
        }
        else if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
        {
            Debug.Log("[TEST] Starting dialogue..");
            StartDialogue(_testDialogue).Forget();
        }
        #endif
    }

    private async UniTaskVoid TestDialogue()
    {
        await UniTask.WaitForSeconds(.7f);
        print("Test Dialogue Started");
        await StartDialogue(_testDialogue);
        print("Test Dialogue Ended");
    }

    public Tween ShowDialogue()
    {
        _bgFadeTween?.Kill(true);
        _bgFadeTween = _background.DOFade(_bgDefaultAlpha, BackgroundFadeDuration)
            .SetLink(_background.gameObject);

        return _bgFadeTween;
    }

    public Tween HideDialogue()
    {
        _subtitleLabel.text = string.Empty;

        _bgFadeTween?.Kill(true);
        _bgFadeTween = _background.DOFade(0, BackgroundFadeDuration)
            .SetLink(_background.gameObject);

        return _bgFadeTween;
    }

    /// <summary>
    /// Starts a new dialogue. If a dialogue is already in progress, it will be cancelled first.
    /// </summary>
    public async UniTask StartDialogue(params DialogueLine[] lines)
    {
        // If still in a dialogue, cancel it
        if (_isInDialogue)
            _dialogueCancellation?.Cancel();

        _dialogueCancellation?.Dispose();
        _dialogueCancellation = new();
        _isInDialogue = true;

        var token = CancellationTokenSource.CreateLinkedTokenSource(_dialogueCancellation.Token, destroyCancellationToken).Token;
        await ShowDialogue().WithCancellation(token);

        foreach (var line in lines)
        {
            var hasAudio = line.Audio != null;
            if (hasAudio)
            {
                _audioSource.volume = _defaultVolume * line.VolumeMultiplier;
                _audioSource.resource = line.Audio;
                _audioSource.Play();
            }

            LineStarted?.Invoke(this, new LineEventArgs { Line = line });
            await UniTask.WaitForSeconds(line.SubtitleStartDelay, cancellationToken: token);

            if (line.WriteInstantly)
            {
                _typewriter.WriteInstantly(line.Line);
            }
            else
            {
                await _typewriter.WriteAsync(line.Line, token);
            }

            // Wait for the audio to finish before moving on to the next line
            if (hasAudio)
            {
                await UniTask.WaitWhile(() => _audioSource.isPlaying, cancellationToken: token);
                _audioSource.volume = _defaultVolume;
            }

            LineCompleted?.Invoke(this, new LineEventArgs { Line = line });
            await UniTask.WaitForSeconds(line.DurationAddition, cancellationToken: token);
        }

        await HideDialogue().WithCancellation(token);
        _isInDialogue = false;
    }

    /// <summary>
    /// Cancels active dialogue.
    /// </summary>
    public void CancelDialogue()
    {
        _dialogueCancellation?.Cancel();
        _isInDialogue = false;
        HideDialogue();
        _audioSource.Stop();
    }

    private void OnPauseStateChanged()
    {
        if (PauseMenu.Instance.IsPaused)
        {
            _audioSource.Pause();
        }
        else
        {
            _audioSource.UnPause();
        }
    }
}
