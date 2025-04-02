using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class InspectMenu : MenuPage
{
    public event Action OverlayClicked;
    private Button _overlayButton;

    protected override void Awake()
    {
        base.Awake();
        _overlayButton = Container.Q<Button>("OverlayButton");
        Assert.IsNotNull(_overlayButton, $"[{name}] Button element named 'OverlayButton' not found");
    }

    protected virtual void OnEnable()
    {
        _overlayButton.clicked += OnOverlayClicked;
    }

    protected virtual void OnDisable()
    {
        _overlayButton.clicked -= OnOverlayClicked;
    }

    private void OnOverlayClicked()
    {
        OverlayClicked?.Invoke();
    }
}
