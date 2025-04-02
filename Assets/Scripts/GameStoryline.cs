using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

using Random = UnityEngine.Random;

public class GameStoryline : MonoBehaviour
{
    #region Singleton
    private static GameStoryline _instance;
    public static GameStoryline Instance {
        get {
            if (_instance == null)
                _instance = FindFirstObjectByType<GameStoryline>(FindObjectsInactive.Include);
            return _instance;
        }
    }
    #endregion

    [SerializeField, Required] private Bomb _bomb;
    [SerializeField, Required] private WalkieTalkie _wt;
    [SerializeField, Required] private ButtonBehaviour _redButton;
    [SerializeField, Required] private Paper _paperOldDefuseGuide;
    [SerializeField, Required] private Paper _paperNewDefuseGuide;
    [SerializeField, Required] private Paper _paperTalkieDestroyed;
    [SerializeField, Required] private Paper _paperEncryptedCrackText;
    [SerializeField, Required] private Paper _paperEmergencyContacts;
    [SerializeField, Required] private Paper _paperRickroll;
    [SerializeField, Required] private Paper _paperMorseCodeClue;
    [SerializeField, Required] private Paper _paperMorseCode;
    [SerializeField, Required] private Numpad _numpad;
    [SerializeField, Required] private Hammer _hammer;
    [SerializeField, Required] private Hammer _secondHammer;
    [SerializeField, Required] private Collider2D _tableCracks;
    [SerializeField, Required] private Transform _rubberDucky;
    [SerializeField, Required] private DefuseButton _fakeDefuseButton1;
    [SerializeField, Required] private DefuseButton _fakeDefuseButton2;
    [SerializeField, Required] private DefuseButton _policeDefuseButton;
    [SerializeField, Required] private AudioSource _policeSiren;
    [SerializeField, Required] private DefuseButton _brokenDefuseButton;
    [SerializeField, Required] private DefuseButton[] _brokenDefuseButtons = new DefuseButton[8];
    [SerializeField] private Bounds _brokenDefuseButtonsBounds;

    [Header("Items Give Positions")]
    [SerializeField, DrawHandle] private Vector3 _numpadGivePos = new(2f, 2f);
    [SerializeField, DrawHandle] private Vector3 _hammerGivePos = new(5.7f, .63f);
    [SerializeField, DrawHandle] private Vector3 _hammer2GivePos = new(2f, 2f);
    [SerializeField, DrawHandle] private Vector3 _newDefuseGuidePaperGivePos = new(2f, 2f);
    [SerializeField, DrawHandle] private Vector3 _morseCodeCluePaperGivePos = new(2f, 2f);
    [SerializeField, DrawHandle] private Vector3 _fakeDefuseButtonGivePos = new(2f, 2f);
    [SerializeField, DrawHandle] private Vector3 _fakeDefuseButton2GivePos = new(2f, 2f);
    [SerializeField, DrawHandle] private Vector3 _policeDefuseButtonGivePos = new(2f, 2f);

    [Space, Title("Dialogues")]
    [SerializeField] private DialogueLine[] _dlgIntroduction;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgForgotDefuseGuide;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgButtonDestroyed;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgNumpadDestroyed;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgDefusedAllegedly;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgStopBombReactivation;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgNormalEnding;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgIdleFromStart;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgForgotEmergencyContacts;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgDestroyEverythingEnding;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgAbazuzyEasterEgg;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgMorseCodeEnding;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgPoliceCalled;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgClickPoliceDefuseButton;
    [Line(GUIColor.Gray)]
    [SerializeField] private DialogueLine[] _dlgClickBrokenDefuseButton;

    [Line(GUIColor.Gray)]
    [Header("Others")]
    [SerializeField] private float _idleFromStartDialogueTriggerTime = 180f;
    [SerializeField] private float _emergencyContactsDialogueTriggerTime = 61f;
    [SerializeField] private float _normalEndingWaitTimeAfterTimerFinished = 40f;
    [SerializeField] private float _policeArriveDelay = 35f;

    public bool IsPlayerIdleFromStart { get => _isPlayerIdleFromStart; set => _isPlayerIdleFromStart = value; }
    public bool IsInDialogue => DialogueManager.Instance.IsInDialogue || _isInStoryDialogue;
    /// <summary>
    /// Returns a list of items that is going to be checked for "Destroy Everything Ending".
    /// </summary>
    public List<GameObject> RegisteredDestructibleItems => _registeredDestructibleItems;

    private CancellationTokenSource _storyDialogueCancellation;
    private bool _isInStoryDialogue;
    private bool _isGameEnded;
    private bool _isPlayerIdleFromStart = true;
    private bool _isRedButtonClickedAfterTimesUp;
    private float _bombTimerFinishedTime = -Mathf.Infinity;
    private List<GameObject> _registeredDestructibleItems = new();
    private List<string> _numpadEnteredCodes = new();
    private bool _gotNumpad;

    // Time based dialogues
    private bool _isIdleFromStartDialogueTriggered;
    private bool _isEmergencyContactsDialogueTriggered;
    private bool _isNormalEndingDialogueTriggered;
    private bool _isBrokenDefuseButtonDialogueTriggered;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {nameof(GameStoryline)} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Assert.IsNotNull(DialogueManager.Instance, $"[{name}] Instance of {nameof(DialogueManager)} not found in scene");
        Assert.IsNotNull(GameOverMenu.Instance, $"[{name}] Instance of {nameof(GameOverMenu)} not found in scene");
    }

    private void OnEnable()
    {
        DialogueManager.Instance.LineStarted += OnDialogueLineStarted;
        DialogueManager.Instance.LineCompleted += OnDialogueLineCompleted;
        _bomb.TimesUp += OnBombTimesUp;
        _bomb.Exploded += OnBombExploded;
        _redButton.Clicked += OnRedButtonClicked;
        _numpad.Menu.Submitted += OnNumpadSubmitted;
        _hammer.Hammered += OnHammered;
        _secondHammer.Hammered += OnHammered;
        _policeDefuseButton.Clicked += OnPoliceDefuseButtonClicked;
        _brokenDefuseButton.Clicked += OnBrokenDefuseButtonClicked;
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance)
        {
            DialogueManager.Instance.LineStarted -= OnDialogueLineStarted;
            DialogueManager.Instance.LineCompleted -= OnDialogueLineCompleted;
        }
        
        _bomb.TimesUp -= OnBombTimesUp;
        _bomb.Exploded -= OnBombExploded;
        _redButton.Clicked -= OnRedButtonClicked;
        _numpad.Menu.Submitted -= OnNumpadSubmitted;
        _hammer.Hammered -= OnHammered;
        _secondHammer.Hammered -= OnHammered;
        _policeDefuseButton.Clicked -= OnPoliceDefuseButtonClicked;
        _brokenDefuseButton.Clicked -= OnBrokenDefuseButtonClicked;
    }

    private void Start()
    {
        // Start the storyline
        Dialogue_Introduction().Forget();
    }

    private void Update()
    {
        // Time Based Dialogues
        if (!IsInDialogue && IsWalkieTalkieActive() && _bomb.gameObject.activeInHierarchy && !_isGameEnded)
        {
            bool timerFinished = _bomb.IsTimerFinished;

            // Idle dialogue
            if (_bomb.TimeLeft <= _idleFromStartDialogueTriggerTime && !timerFinished &&
                IsPlayerIdleFromStart && !_isIdleFromStartDialogueTriggered)
            {
                Debug.Log("Starting idle dialogue..");
                DialogueManager.Instance.StartDialogue(_dlgIdleFromStart).Forget();
                _isIdleFromStartDialogueTriggered = true;
            }

            // Emergency contacts dialogue
            if (_bomb.TimeLeft <= _emergencyContactsDialogueTriggerTime && !timerFinished &&
                !_isEmergencyContactsDialogueTriggered)
            {
                Debug.Log("Starting emergency contacts dialogue..");
                DialogueManager.Instance.StartDialogue(_dlgForgotEmergencyContacts).Forget();
                _isEmergencyContactsDialogueTriggered = true;
            }

            // Normal Ending
            if (TimeSince(_bombTimerFinishedTime) >= _normalEndingWaitTimeAfterTimerFinished && timerFinished && !_isNormalEndingDialogueTriggered)
            {
                Debug.Log("Normal Ending");
                EndGame(EndingID.NormalEnding);
                DialogueManager.Instance.StartDialogue(_dlgNormalEnding).Forget();
                _isNormalEndingDialogueTriggered = true;
            }
        }
    }

    #region Dialogues
    private async UniTaskVoid Dialogue_Introduction()
    {
        if (_isInStoryDialogue)
            CancelActiveStoryDialogue();
        _isInStoryDialogue = true;
            
        _storyDialogueCancellation?.Dispose();
        _storyDialogueCancellation = new();

        // Cancel if this gameObject is destroyed, or if the dialogue is cancelled.
        var token = CancellationTokenSource.CreateLinkedTokenSource(_storyDialogueCancellation.Token, destroyCancellationToken).Token;
        
        // Dialogue 1
        await UniTask.WaitForSeconds(1.4f, cancellationToken: token);
        await DialogueManager.Instance.StartDialogue(_dlgIntroduction);

        // Delay between dialogue 1 and 2
        await UniTask.WaitForSeconds(4f, cancellationToken: token);

        // Dialogue 2
        await DialogueManager.Instance.StartDialogue(_dlgForgotDefuseGuide);
        _isInStoryDialogue = false;
    }
    #endregion

    [Button("Cancel Any Active Story Dialogue")]
    private void CancelActiveStoryDialogue()
    {
        DialogueManager.Instance.CancelDialogue();
        _storyDialogueCancellation?.Cancel();
        _isInStoryDialogue = false;
        _wt.StopVibrating();
    }

    [Button("Give Numpad")]
    private void GiveNumpad()
    {
        if (_gotNumpad)
            return;

        GiveItem(_numpad, _numpadGivePos);
        _gotNumpad = true;
    }

    private void EndGame(EndingID ending, string overrideMessage = null)
    {
        if (_isGameEnded || ending == EndingID.None)
            return;

        var gameOverMenu = GameOverMenu.Instance;
        switch (ending)
        {
            case EndingID.BombExploded:
                gameOverMenu.Setup("GAME OVER", "Bomb Exploded!", overrideMessage ?? "Kaboom.");
                break;
            case EndingID.NormalEnding:
                gameOverMenu.Setup("THE END", "Normal Ending", "You defused the bomb.. kind of.");
                break;
            case EndingID.MorseCommunityEnding:
                gameOverMenu.Setup("THE END", "Morse Community Ending", "The power of beeps and boops!");
                break;
            case EndingID.PoliceEnding:
                gameOverMenu.Setup("THE END", "Police Ending", "You both got captured.");
                break;
            case EndingID.DestroyEverythingEnding:
                gameOverMenu.Setup("THE END", "Destroy Everything Ending", "You destroyed everything.");
                break;
        }

        _isGameEnded = true;
        _numpad.CloseMenu();
        SavedGame.Instance.UnlockedEndings.Add(ending);
        SavedGame.Instance.Save();
        gameOverMenu.Show();
    }

    private bool IsWalkieTalkieActive()
        => _wt.gameObject.activeInHierarchy;

    private void OnBombTimesUp(object sender, EventArgs _)
    {
        _bombTimerFinishedTime = Time.time;

        // Prepare for Normal Ending
        if (IsWalkieTalkieActive())
        {
            CancelActiveStoryDialogue();
            DialogueManager.Instance.StartDialogue(_dlgDefusedAllegedly).Forget();
            return;
        }

        _bomb.Explode("Time's up.");
    }

    private void OnBombExploded(object sender, Bomb.ExplodedEventArgs args)
    {
        _wt.StopVibrating();
        CancelActiveStoryDialogue();

        DOTween.Sequence()
            .AppendInterval(1f)
            .AppendCallback(() => EndGame(EndingID.BombExploded, args.Reason))
            .SetLink(gameObject);
    }

    private void OnDialogueLineStarted(object sender, DialogueManager.LineEventArgs args)
    {
        var line = args.Line;
        switch (line.Name)
        {
            case "BROKEN_DEFUSE_BUTTONS":
                Sequence GiveButton(DefuseButton btn) {
                    btn.transform.localScale *= .5f;
                    btn.transform.position = _wt.transform.position;
                    btn.gameObject.SetActive(true);

                    // Get random pos inside bounds
                    var pos = new Vector3(
                        Random.Range(_brokenDefuseButtonsBounds.min.x, _brokenDefuseButtonsBounds.max.x),
                        Random.Range(_brokenDefuseButtonsBounds.min.y, _brokenDefuseButtonsBounds.max.y),
                        0f
                    );

                    return DOTween.Sequence()
                        .Append(btn.transform.DOMove(pos, 1.5f))
                        .Join(btn.transform.DOScale(1f, .5f))
                        .SetLink(btn.gameObject);
                }

                var sequence = DOTween.Sequence()
                    .AppendInterval(4.8f)
                    .AppendCallback(() => GiveButton(_brokenDefuseButtons[0]))
                    .AppendInterval(1.8f)
                    .AppendCallback(() => GiveButton(_brokenDefuseButtons[1]))
                    .AppendInterval(3.4f)
                    .AppendCallback(() => {
                        for (int i = 2; i < _brokenDefuseButtons.Length; i++)
                            GiveButton(_brokenDefuseButtons[i]);
                    })
                    .SetLink(gameObject);

                break;
        }
    }

    private void OnDialogueLineCompleted(object sender, DialogueManager.LineEventArgs args)
    {
        var line = args.Line;
        switch (line.Name)
        {
            case "BOMB_DEFUSE_GUIDE":
                GiveAndRepositionItem(_paperOldDefuseGuide, _wt.transform.position);
                break;
            case "EMERGENCY_CONTACTS":
                GiveAndRepositionItem(_paperEmergencyContacts, _wt.transform.position);
                break;
            case "POLICE_DEFUSE_BUTTON":
                GiveItem(_policeDefuseButton, _policeDefuseButtonGivePos);
                break;
            case "BROKEN_DEFUSE_BUTTON":
                GiveAndRepositionItem(_brokenDefuseButton, _wt.transform.position);
                break;
        }
    }

    private void OnRedButtonClicked(object sender, EventArgs _)
    {
        if (_bomb.IsDefused)
        {
            return;
        }

        // Morse Code Ending
        if (_numpadEnteredCodes.Contains(NumpadCodes.MorseCodePaper) && _bomb.TimeLeft <= 10 && _bomb.TimeLeft >= 5f)
        {
            CancelActiveStoryDialogue();
            DialogueManager.Instance.StartDialogue(_dlgMorseCodeEnding).Forget();
            EndGame(EndingID.MorseCommunityEnding);
            _bomb.IsDefused = true;
            return;
        }

        // Give numpad if conditions are right
        var timespan = TimeSpan.FromSeconds(_bomb.TimeLeft);
        if (timespan.Minutes.ToString().Contains('1') || timespan.Seconds.ToString().Contains('1'))
        {
            GiveNumpad();
            return;
        }

        if (!_isRedButtonClickedAfterTimesUp && _bomb.IsTimerFinished)
        {
            CancelActiveStoryDialogue();
            DialogueManager.Instance.StartDialogue(_dlgStopBombReactivation).Forget();
            _isRedButtonClickedAfterTimesUp = true;
            return;
        }

        _bomb.Explode("You pressed it, didn't you?");
    }

    private void OnNumpadSubmitted(string input)
    {
        // Codes can't be entered twice
        if (_numpadEnteredCodes.Contains(input))
        {
            _numpad.PlayInvalidCodeSFX();
            return;
        }

        bool invalidCode = false;
        bool walkieTalkieActive = IsWalkieTalkieActive();

        switch (input)
        {


        case NumpadCodes.Hammer:
            GiveItem(_hammer, _hammerGivePos);
            break;
        case NumpadCodes.DefuseGuidePaperV1:
            GiveItem(_paperNewDefuseGuide, _newDefuseGuidePaperGivePos);
            break;
        case NumpadCodes.FakeDefuseButton1:
            GiveItem(_fakeDefuseButton1, _fakeDefuseButtonGivePos);
            break;
        case NumpadCodes.MorseCodePaper:
            GiveItem(_paperMorseCode, _morseCodeCluePaperGivePos);
            break;
        case NumpadCodes.Police:
            if (!walkieTalkieActive || _bomb.IsTimerFinished) {
                invalidCode = true;
                break;
            }
            CancelActiveStoryDialogue();
            DialogueManager.Instance.StartDialogue(_dlgPoliceCalled).Forget();
            break;

        #region Numbers from Emergency Contacts
        case NumpadCodes.Explode1:
        case NumpadCodes.Explode2:
            _bomb.Explode("Oh wait, that wasn't the defuse code?");
            break;
        case NumpadCodes.RubberDucky:
            GiveAndRepositionItem(_rubberDucky, _numpad.transform.position, 2.4f);
            break;
        case NumpadCodes.EasterEgg:
            if (!walkieTalkieActive) {
                invalidCode = true;
                break;
            }
            CancelActiveStoryDialogue();
            DialogueManager.Instance.StartDialogue(_dlgAbazuzyEasterEgg).Forget();
            break;
        case NumpadCodes.MorseCodeCluePaper:
            GiveItem(_paperMorseCodeClue, _morseCodeCluePaperGivePos);
            break;
        case NumpadCodes.FakeDefuseButton2:
            GiveItem(_fakeDefuseButton2, _fakeDefuseButton2GivePos);
            break;
        case NumpadCodes.IncreaseBombTimer:
            if (_bomb.IsTimerFinished) {
                invalidCode = true;
                break;
            }
            _bomb.TimeLeft += 60f;
            break;
        case NumpadCodes.SecondHammer:
            GiveItem(_secondHammer, _hammer2GivePos);
            break;
        case NumpadCodes.FasterBombTimer:
            _bomb.TimerSpeedMultiplier = 3f;
            _bomb.ShowMultiplierLabel();
            break;
        case NumpadCodes.Rickroll:
            GiveAndRepositionItem(_paperRickroll, _numpad.transform.position, 2.4f);
            break;
        #endregion

        default:
            invalidCode = true;
            break;

        }

        if (invalidCode)
        {
            _numpad.PlayInvalidCodeSFX();
        }
        else
        {
            _numpad.Menu.ClearInput();
            _numpad.PlayValidCodeSFX();
            _numpad.CloseMenu();
            if (!_numpadEnteredCodes.Contains(input))
                _numpadEnteredCodes.Add(input);
        }
    }

    private void OnHammered(object sender, Hammer.HammeredEventArgs args)
    {
        var obj = args.HammeredObject;
        bool walkieTalkieActive = IsWalkieTalkieActive();

        // If red button destroyed, start dialogue
        if (obj.TryGetComponent(out ButtonBehaviour redButton) && redButton == _redButton)
        {
            if (!_bomb.IsExploded && walkieTalkieActive) {
                CancelActiveStoryDialogue();
                DialogueManager.Instance.StartDialogue(_dlgButtonDestroyed).Forget();
            }
        }

        // If walkie talkie destroyed, give defuse guide v1.0
        else if (obj.TryGetComponent(out WalkieTalkie _))
        {
            CancelActiveStoryDialogue();
            GiveAndRepositionItem(_paperTalkieDestroyed, _wt.transform.position);
        }

        // If numpad destroyed, start dialogue
        else if (obj.TryGetComponent(out Numpad numpad))
        {
            if (!_bomb.IsExploded && walkieTalkieActive) {
                CancelActiveStoryDialogue();
                DialogueManager.Instance.StartDialogue(_dlgNumpadDestroyed).Forget();
            }
        }

        // If bomb destroyed, explode
        else if (obj.TryGetComponent(out Bomb bomb) && !_bomb.IsDefused)
        {
            bomb.gameObject.SetActive(true);
            bomb.Explode("Mission failed successfully. You blew up!");
        }

        else if (obj == _tableCracks)
        {
            GiveAndRepositionItem(_paperEncryptedCrackText, _tableCracks.transform.position, 1f);
        }

        // Check for Destroy Everything Ending
        int activeCount = _registeredDestructibleItems.Count(x => x.activeInHierarchy);
        Debug.Log($"{activeCount} Object(s) left to destroy to get Destroy Everything Ending.");
        if (activeCount == 0)
        {
            DOVirtual.DelayedCall(2f, () =>
            {
                if (_isGameEnded)
                    return;
                    
                CancelActiveStoryDialogue();
                if (IsWalkieTalkieActive())
                    DialogueManager.Instance.StartDialogue(_dlgDestroyEverythingEnding).Forget();

                EndGame(EndingID.DestroyEverythingEnding);
            });
        }
    }

    private void OnPoliceDefuseButtonClicked()
    {
        if (_bomb.IsDefused)
            return;

        _bomb.IsDefused = true;
        CancelActiveStoryDialogue();
        DialogueManager.Instance.StartDialogue(_dlgClickPoliceDefuseButton).Forget();
        DOTween.Sequence()
            .AppendInterval(_policeArriveDelay)
            .AppendCallback(() => {
                if (!_isGameEnded) {
                    _policeSiren.volume = 0;
                    _policeSiren.Play();
                    _policeSiren.DOFade(.4f, 7f)
                        .SetEase(Ease.InQuint)
                        .SetDelay(1f)
                        .OnComplete(() => EndGame(EndingID.PoliceEnding))
                        .SetLink(gameObject);
                }
            })
            .SetLink(gameObject);
    }

    private void OnBrokenDefuseButtonClicked()
    {
        if (!_isBrokenDefuseButtonDialogueTriggered)
        {
            _isBrokenDefuseButtonDialogueTriggered = true;
            CancelActiveStoryDialogue();
            DialogueManager.Instance.StartDialogue(_dlgClickBrokenDefuseButton).Forget();
        }
    }

    private static void GiveAndRepositionItem(Component item, Vector3 startPos, float moveAmount = 2f)
    {
        var itemTransform = item.transform;
        itemTransform.SetParent(null);
        itemTransform.position = startPos;
        itemTransform.localScale = Vector3.one * .5f;
        itemTransform.gameObject.SetActive(true);

        // Tween move
        var dir = (Vector3.zero - itemTransform.position).normalized;
        DOTween.Sequence()
            .Append(itemTransform.DOMove(itemTransform.position + dir * moveAmount, .5f))
            .Join(itemTransform.DOScale(1f, .5f))
            .SetLink(itemTransform.gameObject);
    }

    private static void GiveItem(Component item, Vector3 givePos)
    {
        item.gameObject.SetActive(true);
        item.transform.DOMove(givePos, 10f)
            .SetEase(Ease.OutSine)
            .SetSpeedBased()
            .SetLink(item.gameObject);
    }

    private static float TimeSince(float since)
        => Time.time - since;
}
