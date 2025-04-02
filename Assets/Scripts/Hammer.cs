using System;
using UnityEngine;
using EditorAttributes;
using DG.Tweening;
using System.Collections.Generic;

public interface IHammerHandler
{
    public void OnHammered(Hammer hammer);
}

public class Hammer : MonoBehaviour
{
    [SerializeField, Required] private DragBehaviour _dragBehaviour;
    [SerializeField, Required] private BoxCollider2D _collider;
    [SerializeField, Required] private BoxCollider2D _hitbox;
    [SerializeField] private Bounds _hammerBounds = new(Vector3.zero, Vector3.one);

    [Space]
    [SerializeField] private ParticleSystem _fxHammer;
    [SerializeField] private AudioSource _sfxHammer;

    public event EventHandler<HammeredEventArgs> Hammered;
    public class HammeredEventArgs : EventArgs { public Collider2D HammeredObject; }
    
    private Sequence _swingSequence;
    private float _defaultRotationZ;

    private void Awake()
    {
        _defaultRotationZ = transform.localEulerAngles.z;
    }

    private void OnEnable()
    {
        _dragBehaviour.DragEnded += OnDragEnded;
    }

    private void OnDisable()
    {
        _dragBehaviour.DragEnded -= OnDragEnded;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(_hammerBounds.center, _hammerBounds.size);
    }

    private void OnDragEnded(object sender, DragBehaviour.PointerEventArgs _)
    {
        // Collider2D[] others = Physics2D.OverlapBoxAll(transform.position + _hammerBounds.center, _hammerBounds.size, _collider.transform.eulerAngles.z, _hammerableLayers);

        List<Collider2D> others = new(4);
        _hitbox.Overlap(others);

        foreach (var other in others)
        {
            if (other == _collider)
                continue;

            Debug.Log($"Hammering '{other.name}'");
            var swingHammerPos = other.transform.position + new Vector3(2.7f, .6f, 0);

            _sfxHammer.Play();
            _swingSequence?.Kill();
            _swingSequence = DOTween.Sequence()
                .AppendCallback(() => _dragBehaviour.enabled = false)
                .Append(transform.DOMove(swingHammerPos, .3f))
                .Join(transform.DOLocalRotate(new(0, 0, -45f), .3f).SetEase(Ease.OutQuint))
                .Append(transform.DOLocalRotate(new(0, 0, 90f), .2f).SetEase(Ease.InQuint))
                .AppendCallback(() => {
                    _fxHammer.Play();
                    Camera2D.Current.AddShake(1f, 8f, .8f);
                    _dragBehaviour.enabled = true;
                    if (other.TryGetComponent(out IHammerHandler handler))
                        handler.OnHammered(this);

                    other.gameObject.SetActive(false);
                    Hammered?.Invoke(this, new HammeredEventArgs() { HammeredObject = other });
                })
                .AppendInterval(.2f)
                .Append(transform.DOLocalRotate(new(0, 0, _defaultRotationZ), .4f).SetEase(Ease.InOutSine))
                .SetLink(gameObject);

        }
    }
}
