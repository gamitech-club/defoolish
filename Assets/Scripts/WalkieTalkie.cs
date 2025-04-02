using System;
using System.Collections;
using UnityEngine;
using EditorAttributes;
using DG.Tweening;
using UnityEngine.Assertions;

public class WalkieTalkie : MonoBehaviour
{
    [SerializeField, Required] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _vibrateStrength = .05f;
    [SerializeField] private int _vibrateFrequency = 13;

    private Tween _shakeTween;

    private void Awake()
    {
        Assert.IsNotNull(DialogueManager.Instance, $"[{name}] Instance of {nameof(DialogueManager)} not found in scene");
    }

    private void OnEnable()
    {
        DialogueManager.Instance.LineStarted += OnDialogueLineStarted;
        DialogueManager.Instance.LineCompleted += OnDialogueLineCompleted;
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance)
        {
            DialogueManager.Instance.LineStarted -= OnDialogueLineStarted;
            DialogueManager.Instance.LineCompleted -= OnDialogueLineCompleted;
        }
    }

    public void StartVibrating()
    {
        _shakeTween?.Kill(true);
        _shakeTween = _spriteRenderer.transform.DOShakePosition(.5f, _vibrateStrength, _vibrateFrequency, fadeOut: false)
            .SetLink(_spriteRenderer.gameObject)
            .SetLoops(-1)
            .OnKill(() => {
                if (_spriteRenderer)
                    _spriteRenderer.transform.localPosition = Vector3.zero;
            });
    }

    public void StopVibrating()
    {
        _shakeTween?.Kill(true);
        _shakeTween = null;
    }

    private void OnDialogueLineStarted(object sender, DialogueManager.LineEventArgs _)
    {
        StartVibrating();
    }

    private void OnDialogueLineCompleted(object sender, DialogueManager.LineEventArgs _)
    {
        StopVibrating();
    }
}
