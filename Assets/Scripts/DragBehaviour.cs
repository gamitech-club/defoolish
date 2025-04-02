using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using EditorAttributes;
using DG.Tweening;

public class DragBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SortingGroup _sortingGroup;
    [SerializeField, EnableField(nameof(SpriteAssigned))] private float _spriteDragScaleMultiplier = 1.1f;

    [Header("SFX")]
    [SerializeField] private AudioSource _sfxDragStart;
    [SerializeField] private AudioSource _sfxDragEnd;

    public event EventHandler<PointerEventArgs> Clicked;
    public event EventHandler<PointerEventArgs> DragStarted;
    public event EventHandler<PointerEventArgs> Dragging;
    public event EventHandler<PointerEventArgs> DragEnded;
    public class PointerEventArgs : EventArgs {
        public PointerEventData EventData;
        public PointerEventArgs(PointerEventData eventData) => EventData = eventData;
    }

    public bool IsDragging => _isDragging;
    public bool SpriteAssigned => _spriteRenderer != null;

    private static readonly string DraggedSortingLayer = "Dragged Object";
    private Vector2 _dragStartOffset;
    private Tween _scaleTween;
    private Vector2 _spriteDefaultScale;
    private bool _isDragging;
    private string _defaultSortingLayer;

    private void Awake()
    {
        if (_spriteRenderer)
        {
            _spriteDefaultScale = _spriteRenderer.transform.localScale;
        }

        if (_sortingGroup)
        {
            _defaultSortingLayer = _sortingGroup.sortingLayerName;
        }
    }

    private void OnDisable()
    {
        // Stop dragging if gameobject is disabled
        if (_isDragging)
        {
            _isDragging = false;
            _scaleTween?.Kill();
            transform.localScale = _spriteDefaultScale;
            if (_sortingGroup)
                _sortingGroup.sortingLayerName = _defaultSortingLayer;

            CursorManager.Instance.SetDefaultCursor();
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _dragStartOffset = Camera2D.Current.ScreenToWorldPoint(eventData.position) - (Vector2)transform.position;
        CursorManager.Instance.SetHoldCursor();
        if (_sfxDragStart) _sfxDragStart.Play();

        // Scale sprite
        if (_spriteRenderer)
        {
            if (_sortingGroup)
                _sortingGroup.sortingLayerName = DraggedSortingLayer;

            _scaleTween?.Kill();
            _scaleTween = _spriteRenderer.transform.DOScale(_spriteDefaultScale * _spriteDragScaleMultiplier, .15f)
                .SetEase(Ease.OutCubic)
                .SetLink(_spriteRenderer.gameObject);
        }

        if (GameStoryline.Instance)
            GameStoryline.Instance.IsPlayerIdleFromStart = false;

        DragStarted?.Invoke(this, new(eventData));
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        CursorManager.Instance.SetDefaultCursor();
        if (_sfxDragEnd) _sfxDragEnd.Play();

        // Scale sprite
        if (_spriteRenderer)
        {
            if (_sortingGroup)
                _sortingGroup.sortingLayerName = _defaultSortingLayer;
                
            _scaleTween?.Kill();
            _scaleTween = _spriteRenderer.transform.DOScale(_spriteDefaultScale, .15f)
                .SetEase(Ease.InSine)
                .SetLink(_spriteRenderer.gameObject);
        }

        DragEnded?.Invoke(this, new(eventData));
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        transform.position = Camera2D.Current.ScreenToWorldPoint(eventData.position) - _dragStartOffset;
        Dragging?.Invoke(this, new(eventData));
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (!_isDragging && eventData.button == PointerEventData.InputButton.Left)
        {
            Clicked?.Invoke(this, new(eventData));
            Debug.Log($"Clicked {name}");
        }
    }
}
