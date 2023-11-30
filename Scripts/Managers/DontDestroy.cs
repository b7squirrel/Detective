using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        var obj = FindObjectsOfType<DontDestroy>();
        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i] != this)
            {
                if(obj[i].gameObject.name == gameObject.name)
                {
                    Destroy(gameObject);
                }
            }
        }

        DontDestroyOnLoad(gameObject);
    }
}
