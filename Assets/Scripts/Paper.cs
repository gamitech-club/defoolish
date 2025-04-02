using UnityEngine;
using EditorAttributes;
using UnityEngine.UIElements;

[RequireComponent(typeof(InspectableBehaviour))]
public class Paper : MonoBehaviour
{
    [SerializeField, Required] private InspectableBehaviour _inspectable;
    [SerializeField, TextArea(1, 15)] private string _content = "Hello World";

    private void Reset()
    {
        _inspectable = GetComponent<InspectableBehaviour>();
    }

    private void Start()
    {
        var label = _inspectable.Menu.Container.Q<Label>("ContentLabel");
        if (label != null) {
            label.text = _content;
        } else {
            Debug.LogError($"[{name}] Label element named 'ContentLabel' not found", this);
        }
    }
}
