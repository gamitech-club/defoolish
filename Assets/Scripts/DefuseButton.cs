using System;
using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(ButtonBehaviour))]
public class DefuseButton : MonoBehaviour
{
    public enum PressBehaviour
    {
        None,
        Defuse,
        Explode
    };

    [SerializeField] private PressBehaviour _pressBehaviour = PressBehaviour.Defuse;
    [SerializeField, EnableField(nameof(_pressBehaviour), PressBehaviour.Explode)]
    private string _explodeReason = "Not all buttons are your friends.";

    public event Action Clicked;
    public PressBehaviour PressAction { get => _pressBehaviour; set => _pressBehaviour = value; }

    private void OnEnable()
    {
        GetComponent<ButtonBehaviour>().Clicked += OnClicked;
    }

    private void OnDisable()
    {
        GetComponent<ButtonBehaviour>().Clicked -= OnClicked;
    }

    private void OnClicked(object sender, EventArgs _)
    {
        if (!Bomb.Instance)
        {
            Debug.LogWarning($"[{name}] There is no bomb in the scene.", this);
            return;
        }

        switch (_pressBehaviour)
        {
            case PressBehaviour.Defuse:
                Bomb.Instance.IsDefused = true;
                break;
            case PressBehaviour.Explode:
                if (!Bomb.Instance.IsDefused)
                    Bomb.Instance.Explode(_explodeReason);
                break;
        }

        Clicked?.Invoke();
    }
}
