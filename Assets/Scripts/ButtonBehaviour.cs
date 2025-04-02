using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioSource _sfxPress;

    public event EventHandler Clicked;
    private DragBehaviour _dragBehaviour;

    private void Awake()
    {
        _dragBehaviour = GetComponent<DragBehaviour>();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || (_dragBehaviour && _dragBehaviour.IsDragging))
            return;
        
        if (GameStoryline.Instance)
            GameStoryline.Instance.IsPlayerIdleFromStart = false;
            
        _sfxPress.Play();
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
