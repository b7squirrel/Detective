using UnityEngine;

public class VictoryPanel : MonoBehaviour
{
    public void PauseTime()
    {
        FindObjectOfType<PauseManager>().PauseGame();
    }
}