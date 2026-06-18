using UnityEngine;

public class DebugOnlyObject : MonoBehaviour
{
    private void Awake()
    {
        GameConfig config = Resources.Load<GameConfig>("GameConfig");
        if (config == null)
        {
            Debug.LogWarning("GameConfig not found in Resources!");
            return;
        }

        gameObject.SetActive(config.isDebugMode);
    }
}