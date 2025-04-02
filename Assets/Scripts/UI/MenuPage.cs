using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Assertions;
using EditorAttributes;
using DG.Tweening;

public class MenuPage : MonoBehaviour
{
    public enum AnimationStyle
    {
        None,
        PushLeft,
        PushRight,
        PushUp,
        PushDown,
        Dissolve
    }

    [Serializable]
    public struct ButtonMenuPair
    {
        public string ButtonName;
        public MenuPage MenuPage;
        public AnimationStyle AnimationOverride;

        public ButtonMenuPair(string buttonName, MenuPage menuPage)
        {
            ButtonName = buttonName;
            MenuPage = menuPage;
            AnimationOverride = AnimationStyle.None;
        }
    }

    [Header("Menu Page")]
    [SerializeField, Required] private UIDocument _uiDocument;
    [SerializeField] private string _containerName = "Container";
    [SerializeField] private AnimationStyle _animation;
    [SerializeField] private float _transitionDuration = 0.3f;
    [SerializeField] private bool _shouldStartHidden;
    [SerializeField] private ButtonMenuPair[] _buttonMenuPairs;

    #if UNITY_EDITOR
    [SerializeField, Line(GUIColor.Gray), HideField(nameof(__editorTrue))]
    private bool __editorLine;
    private bool __editorTrue = true;
    #endif

    public bool IsHidden => _isHidden;
    public VisualElement Container => _container;
    public AnimationStyle Animation { get => _animation; set => _animation = value; }
    public float TransitionDuration => _transitionDuration;
    public ButtonMenuPair[] ButtonMenuPairs {  get => _buttonMenuPairs; set => _buttonMenuPairs = value; }

    public event Action Showed;
    public event Action Hid;
    
    private bool _isHidden;
    private VisualElement _container;
    private VisualElement _lastFocusedElement;
    private Sequence _delayedDisableSequence;

    protected virtual void Awake()
    {
        _uiDocument.enabled = true;
        _container = _uiDocument.rootVisualElement.Q(_containerName);
        Assert.IsNotNull(_container, $"[{name}] Container element named '{_containerName}' not found");
    }

    protected virtual void Start()
    {
        RegisterButtonMenuPairs();
        if (_shouldStartHidden)
            Hide();

        _container.style.transitionProperty = new(new List<StylePropertyName> { "translate", "opacity" });
        _container.style.transitionDuration = new(new List<TimeValue> { _transitionDuration, _transitionDuration });
    }

    private void RegisterButtonMenuPairs()
    {
        foreach (var pair in _buttonMenuPairs)
        {
            Button button = _container.Q<Button>(pair.ButtonName);
            if (button != null)
            {
                button.clicked += () =>
                {
                    _lastFocusedElement = button;

                    foreach (var otherPair in _buttonMenuPairs)
                    {
                        if (otherPair.MenuPage != pair.MenuPage)
                            otherPair.MenuPage.Hide();
                    }

                    Hide(pair.AnimationOverride == AnimationStyle.None ? _animation : pair.AnimationOverride);
                    pair.MenuPage.Show();
                    pair.MenuPage.TryFocus();
                };
            }
            else
            {
                Debug.LogWarning($"[{name}] Button element named '{pair.ButtonName}' not found", this);
            }
        }
    }

    protected void OpenMenu(MenuPage menu, bool hideSelf = true)
    {
        if (menu == null)
            throw new ArgumentNullException(nameof(menu));

        foreach (var pair in _buttonMenuPairs)
        {
            if (pair.MenuPage != menu)
                pair.MenuPage.Hide();
        }

        if (hideSelf)
            Hide();

        menu.Show();
        menu.TryFocus();
    }

    public virtual void Hide(AnimationStyle animation)
    {
        _delayedDisableSequence?.Kill();
        _delayedDisableSequence = DOTween.Sequence()
            .AppendInterval(_transitionDuration)
            .AppendCallback(() => _container.enabledSelf = false)
            .SetLink(_uiDocument.gameObject)
            .SetUpdate(true);

        switch (animation)
        {
            case AnimationStyle.None:
                _container.visible = false;
                break;
            case AnimationStyle.PushLeft:
                _container.style.translate = new(new Translate(new Length(-100f, LengthUnit.Percent), 0));
                break;
            case AnimationStyle.PushRight:
                _container.style.translate = new(new Translate(new Length(100f, LengthUnit.Percent), 0));
                break;
            case AnimationStyle.PushUp:
                _container.style.translate = new(new Translate(0, new Length(-100f, LengthUnit.Percent)));
                break;
            case AnimationStyle.PushDown:
                _container.style.translate = new(new Translate(0, new Length(100f, LengthUnit.Percent)));
                break;
            case AnimationStyle.Dissolve:
                _container.style.opacity = 0f;
                _delayedDisableSequence.AppendCallback(() => _container.style.display = DisplayStyle.None);
                break;
        }

        _isHidden = true;
        Hid?.Invoke();
    }

    public virtual void Show()
    {
        if (!_uiDocument.enabled)
            _uiDocument.enabled = true;

        _delayedDisableSequence?.Kill();
        _container.enabledSelf = true;

        // Reset styles and let UI Toolkit transition do its thing
        _container.visible = true;
        _container.style.translate = StyleKeyword.Null;
        _container.style.opacity = StyleKeyword.Null;
        _container.style.display = StyleKeyword.Null;

        _isHidden = false;
        Showed?.Invoke();
    }

    public void Hide()
        => Hide(_animation);

    public void TryFocus()
    {
        _lastFocusedElement ??= _container.Query().Where(x => x.focusable).First();

        if (_lastFocusedElement != null) {
            _lastFocusedElement.Focus();
        } else {
            Debug.Log($"[{name}] No focusable elements found");
        }
    }
}
