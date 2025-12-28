using UnityEngine;

public class StageTime : MonoBehaviour
{
    float elaspedTime;

    public void Init(float _time)
    {
        FindObjectOfType<WallManager>().SetStageDuration(_time);
    }

    void Update()
    {
        elaspedTime += Time.deltaTime;
    }
    public float GetElapsedTime() => elaspedTime;
}
