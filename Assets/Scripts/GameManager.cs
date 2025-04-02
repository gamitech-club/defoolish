using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance {
        get {
            if (_instance == null)
                _instance = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
            return _instance;
        }
    }
    #endregion

    private void Awake()
    {
        // If there's already an instance, destroy this one.
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {nameof(GameManager)} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }
    }
}
