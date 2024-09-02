using UnityEngine;

public class ToggleGameObject : MonoBehaviour
{
    [SerializeField] GameObject go;
    [SerializeField] bool startEnabled;
    bool isEnabled;

    private void Start()
    {
        isEnabled = startEnabled;
    }

    public void ToggleGo()
    {
        isEnabled = !isEnabled;
        go.SetActive(isEnabled);
    }
}
