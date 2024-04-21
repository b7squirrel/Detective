using UnityEngine;
using UnityEngine.UI;

public class BGScroller : MonoBehaviour
{
    [SerializeField] RawImage image;
    [SerializeField] float x, y;

    private void Update()
    {
        image.uvRect = 
            new Rect(image.uvRect.position + new Vector2(x, y) * Time.deltaTime, image.uvRect.size);
    }
}