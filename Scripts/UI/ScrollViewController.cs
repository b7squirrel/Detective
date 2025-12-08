using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    ScrollRect scrollRect;
    RectTransform gridLayout;
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        gridLayout = GetComponentInChildren<GridLayoutGroup>().GetComponent<RectTransform>();
    }

    private void Update() {
        UpdateScrollView();
    }

    public void UpdateScrollView()
    {
        int rowNum = gridLayout.childCount / 4;
        Debug.Log("row num = " + rowNum);
        if(rowNum == 0) return;
        float height = rowNum * 350f;
        scrollRect.content.sizeDelta = 
            new Vector2(scrollRect.content.sizeDelta.x, height);
    }
}
