using UnityEngine;
using EditorAttributes;
using UnityEngine.UIElements;

public class Numpad : MonoBehaviour
{
    [SerializeField, Required] private InspectableBehaviour _inspectable;
    [SerializeField, Required] private NumpadMenu _menu;

    [Header("SFXs")]
    [SerializeField] private AudioSource _sfxSubmitValid;
    [SerializeField] private AudioSource _sfxSubmitInvalid;
    [SerializeField] private AudioSource _sfxKeyPress;

    public NumpadMenu Menu => _menu;

    private void Reset()
    {
        _inspectable = GetComponent<InspectableBehaviour>();
        _menu = GetComponentInChildren<NumpadMenu>();
    }

    private void Start()
    {
        _menu.Container.Query<Button>()
            .Where(x => !x.name.StartsWith("Overlay"))
            .ForEach(x => x.clicked += OnAnyButtonClicked);
    }

    public void CloseMenu()
        => _inspectable.Close();
    
    public void PlayValidCodeSFX()
    {
        if (_sfxSubmitValid)
            _sfxSubmitValid.Play();
    }

    public void PlayInvalidCodeSFX()
    {
        if (_sfxSubmitInvalid)
            _sfxSubmitInvalid.Play();
    }

    private void OnAnyButtonClicked()
    {
        if (_sfxKeyPress)
        {
            _sfxKeyPress.pitch = Random.Range(.9f, 1.1f);
            _sfxKeyPress.Play();
        }
    }
}
