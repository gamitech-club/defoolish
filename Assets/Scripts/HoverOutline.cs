using UnityEngine;
using EditorAttributes;
using UnityEngine.Assertions;

public class HoverOutline : MonoBehaviour
{
    [SerializeField, Required] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _thicknessMultiplier = 1f;

    const string OutlineEnabledProperty  = "_OutlineEnabled";
    static readonly int _outlineEnabledID = Shader.PropertyToID(OutlineEnabledProperty);
    static readonly int _outlineThicknessID = Shader.PropertyToID("_OutlineThickness");

    private Material _material;

    private void Awake()
    {
        _material = _spriteRenderer.material;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (_material.HasInt(_outlineEnabledID))
        {
            _material.SetInt(_outlineEnabledID, 0);
            _material.SetFloat(_outlineThicknessID, _material.GetFloat(_outlineThicknessID) * _thicknessMultiplier);
        }
        else
        {
            Debug.LogError($"[{name}] Material {_material.name} has no property {OutlineEnabledProperty}", this);
        }
    }

    void OnMouseEnter()
    {
        _material.SetInt(_outlineEnabledID, 1);
    }

    void OnMouseExit()
    {
        _material.SetInt(_outlineEnabledID, 0);
    }
}
