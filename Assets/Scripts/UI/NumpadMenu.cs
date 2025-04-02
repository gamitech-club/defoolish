using System;
using UnityEngine;
using UnityEngine.UIElements;

public class NumpadMenu : InspectMenu
{
    public event Action NumberPressed;
    public event Action<string> Submitted;

    private TextField _inputField;

    protected override void Awake()
    {
        base.Awake();
        _inputField = Container.Q<TextField>();
        Container.Q<Button>("DeleteButton").clicked += OnDeleteButtonClicked;
        Container.Q<Button>("SubmitButton").clicked += OnSubmitButtonClicked;

        // Register numpad keys (name starts with "Numpad")
        Container.Query<Button>().Where(x => x.name.StartsWith("Numpad")).ForEach(x => {
            var value = x.text;
            x.clicked += () => {
                if (_inputField.value.Length < _inputField.maxLength)
                {
                    _inputField.value += value;
                    NumberPressed?.Invoke();
                }
            };
        });
    }

    protected override void Start()
    {
        base.Start();
    }

    public void ClearInput()
    {
        _inputField.value = string.Empty;
    }

    private void OnDeleteButtonClicked()
    {
        if (_inputField.value.Length > 0)
            _inputField.value = _inputField.value[..^1];
    }

    private void OnSubmitButtonClicked()
    {
        Submitted?.Invoke(_inputField.value);
    }
}
