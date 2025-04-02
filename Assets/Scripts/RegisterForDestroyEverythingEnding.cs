using UnityEngine;

public class RegisterForDestroyEverythingEnding : MonoBehaviour
{
    private void Start()
    {
        if (GameStoryline.Instance != null)
            GameStoryline.Instance.RegisteredDestructibleItems.Add(gameObject);
    }
}
