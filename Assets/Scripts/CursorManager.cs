using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("Cursor Textures")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D handCursor;
    [SerializeField] private Texture2D holdCursor;

    [Header("Cursor Offsets (Hotspot)")]
    [SerializeField] private Vector2 defaultCursorOffset = Vector2.zero;
    [SerializeField] private Vector2 handCursorOffset = Vector2.zero;
    [SerializeField] private Vector2 holdCursorOffset = Vector2.zero;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetDefaultCursor();
    }

    private void SetCursor(Texture2D cursorTexture, Vector2 offset)
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, offset, CursorMode.Auto);
        }
        else
        {
            Debug.LogWarning("Cursor texture is missing!");
        }
    }

    public void SetDefaultCursor()
    {
        SetCursor(defaultCursor, defaultCursorOffset);
    }

    public void SetHandCursor()
    {
        SetCursor(handCursor, handCursorOffset);
    }

    public void SetHoldCursor()
    {
        SetCursor(holdCursor, holdCursorOffset);
    }
}
