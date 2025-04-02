using System;
using UnityEngine;
using UnityEngine.Audio;
using EditorAttributes;
using DG.Tweening;
using TMPro;

public class BombVisual : MonoBehaviour
{
    [SerializeField, Required] private Bomb _bomb;
    [SerializeField, Required] private TextMeshPro _timerLabel;
    [SerializeField] private string _timerFormat = "mm\\:ss";

    [Header("Tick")]
    [SerializeField, Required] private AudioSource _sfxTick;
    [SerializeField] private AudioResource[] _sfxTicks;

    [Header("Explosion")]
    [SerializeField, Required] private AudioSource _sfxExplode;
    [SerializeField, Required] private ParticleSystem _fxExplode;

    private TimeSpan _timeLeft;
    private int _timerSeconds;

    private void Reset()
    {
        _bomb = GetComponent<Bomb>();
    }

    private void OnEnable()
    {
        _bomb.Exploded += OnExploded;
    }

    private void OnDisable()
    {
        _bomb.Exploded -= OnExploded;
    }

    // Update is called once per frame
    void Update()
    {
        HandleTicking();
        HandleTimerLabel();
    }

    private void HandleTicking()
    {
        _timeLeft = TimeSpan.FromSeconds(_bomb.TimeLeft);

        var seconds = _timeLeft.Seconds;
        if (seconds != _timerSeconds)
        {
            // Tick SFX
            _sfxTick.resource = _sfxTicks[UnityEngine.Random.Range(0, _sfxTicks.Length)];
            _sfxTick.Play();

            // Punch scale
            _timerLabel.transform.DOPunchScale(Vector3.one * .1f, .25f, 8);
        }

        _timerSeconds = seconds;
    }

    private void HandleTimerLabel()
    {
        _timerLabel.text = _timeLeft.ToString(_timerFormat);
    }

    private void OnExploded(object sender, EventArgs e)
    {
        _sfxExplode.transform.SetParent(null);
        _sfxExplode.Play();
        _fxExplode.transform.SetParent(null);
        _fxExplode.Play();
        Camera2D.Current.AddShake(3f, 16f, 1f);
    }
}
