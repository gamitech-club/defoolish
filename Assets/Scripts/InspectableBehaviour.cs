using System;
using UnityEngine;
using EditorAttributes;

[RequireComponent(typeof(DragBehaviour))]
public class InspectableBehaviour : MonoBehaviour
{
    [SerializeField, Required] private DragBehaviour _dragBehaviour;
    [SerializeField, Required] private InspectMenu _menu;
    [SerializeField] private bool _closeOnBombExploded = true;

    [Header("SFX")]
    [SerializeField] private AudioSource _sfxOpen;
    [SerializeField] private AudioSource _sfxClose;

    public event EventHandler Opened;
    public event EventHandler Closed;

    public bool IsOpened => _isOpened;
    public InspectMenu Menu => _menu;

    private bool _isOpened;

    private void Reset()
    {
        _dragBehaviour = GetComponent<DragBehaviour>();
        _menu = GetComponentInChildren<InspectMenu>();
    }

    private void OnEnable()
    {
        _dragBehaviour.Clicked += OnClicked;
        _menu.OverlayClicked += OnOverlayClicked;

        if (Bomb.Instance)
            Bomb.Instance.Exploded += OnBombExploded;
    }

    private void OnDisable()
    {
        _dragBehaviour.Clicked -= OnClicked;
        _menu.OverlayClicked -= OnOverlayClicked;

        if (Bomb.Instance)
            Bomb.Instance.Exploded -= OnBombExploded;
    }

    private void Open()
    {
        if (_isOpened)
            return;

        _isOpened = true;
        _menu.Show();
        if (_sfxOpen)
            _sfxOpen.Play();

        var cam = Camera2D.Current;
        cam.FocusTemporarily(transform);
        cam.Zoom(3f);

        Opened?.Invoke(this, EventArgs.Empty);
    }

    public void Close()
    {
        if (!_isOpened)
            return;

        _isOpened = false;
        _menu.Hide();
        if (_sfxClose)
            _sfxClose.Play();

        var cam = Camera2D.Current;
        cam.CancelTempFocus();
        cam.Zoom(cam.DefaultOrthoSize);

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void OnClicked(object sender, DragBehaviour.PointerEventArgs args)
    {
        if (!Camera2D.Current.IsTempFocus)
        {
            if (GameStoryline.Instance)
                GameStoryline.Instance.IsPlayerIdleFromStart = false;
                
            Open();
        }
    }

    private void OnOverlayClicked()
    {
        Close();
    }

    private void OnBombExploded(object sender, EventArgs e)
    {
        if (_closeOnBombExploded)
            Close();
    }
}
