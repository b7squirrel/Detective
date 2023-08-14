using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    ScrollRect scrollRect;
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    public void UpdateScrollView(int rowNum)
    {
        if(rowNum == 0) return;
        float height = rowNum * 300f;
        scrollRect.content.sizeDelta = 
            new Vector2(scrollRect.content.sizeDelta.x, height);
    }
}
