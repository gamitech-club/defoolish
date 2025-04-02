using System;
using UnityEngine;
using EditorAttributes;
using DG.Tweening;
using TMPro;

public class Bomb : MonoBehaviour
{
    #region Singleton
    private static Bomb _instance;
    public static Bomb Instance {
        get {
            if (_instance == null)
                _instance = FindFirstObjectByType<Bomb>(FindObjectsInactive.Include);
            return _instance;
        }
    }
    #endregion

    [SerializeField, Required] private DragBehaviour _dragBehaviour;
    [SerializeField, Required] private TextMeshPro _timerSpeedMultiplierLabel;
    [SerializeField] private float _timeLeft = 30f;
    [SerializeField] private float _timerSpeedMultiplier = 1f;
    [SerializeField] private float _dragExplodeSpeed = 25f;
    [SerializeField] private float _maxMoveDistance = 10f; // Max allowed movement distance before explosion
    [SerializeField] private bool _isTimerPaused;

    public event EventHandler TimesUp;
    public event EventHandler<ExplodedEventArgs> Exploded;
    public class ExplodedEventArgs : EventArgs { public string Reason; }

    public float TimeLeft { get => _timeLeft; set => _timeLeft = value; }
    public float TimerSpeedMultiplier { get => _timerSpeedMultiplier; set => _timerSpeedMultiplier = value; }
    public bool IsDefused { get => _isDefused; set => _isDefused = value; }
    public bool IsExploded => _isExploded;
    public bool IsTimerFinished => _isTimerFinished;

    private bool _isExploded;
    private bool _isTimerFinished;
    private bool _isDefused;
    private Vector3 _initialPosition; // Store the first position

    private void Awake()
    {
        // If there's already an instance, destroy this one
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {nameof(Bomb)} found. Destroying the new one.");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        _dragBehaviour.Dragging += OnDrag;
    }

    private void OnDisable()
    {
        _dragBehaviour.Dragging -= OnDrag;
    }

    private void Start()
    {
        _initialPosition = transform.position; // Save the original position
    }

    private void Update()
    {
        HandleTimer();
        HandleDistanceCheck(); // Continuously check if the bomb has moved too far
    }

    private void HandleTimer()
    {
        if (_isExploded || _isTimerPaused || _isDefused)
            return;

        var delta = Time.deltaTime * _timerSpeedMultiplier;
        var willEnd = _timeLeft > 0 && _timeLeft - delta <= 0;
        _timeLeft -= delta;

        if (willEnd)
        {
            _isTimerPaused = true;
            _isTimerFinished = true;
            TimesUp?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HandleDistanceCheck()
    {
        if (_isExploded || _isDefused)
            return;

        float sqrDistance = (transform.position - _initialPosition).sqrMagnitude;
        if (sqrDistance > _maxMoveDistance * _maxMoveDistance)
        {
            Explode($"Bomb moved too far.");
        }
    }

    public void Explode(string reason = "Oops.")
    {
        if (_isExploded)
            return;

        _isExploded = true;
        Debug.Log($"<color=#ff5959>Bomb Exploded!</color> Reason: {reason}");
        Exploded?.Invoke(this, new() { Reason = reason });
        gameObject.SetActive(false);
    }

    public void ShowMultiplierLabel()
    {
        _timerSpeedMultiplierLabel.text = $"x{_timerSpeedMultiplier}";
        _timerSpeedMultiplierLabel.gameObject.SetActive(true);
    }

    private void OnDrag(object sender, DragBehaviour.PointerEventArgs args)
    {
        // Explode if dragged too fast
        if (!_isExploded && !_isDefused && args.EventData.delta.magnitude > _dragExplodeSpeed)
        {
            Explode("Bomb moved too fast.");
        }
    }
}
