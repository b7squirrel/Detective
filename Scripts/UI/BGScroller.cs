using UnityEngine;
using UnityEngine.UI;

public class BGScroller : MonoBehaviour
{
    [SerializeField] RawImage image;
    [SerializeField] float x, y;
    [SerializeField] float moveSpeed;

    private void Update()
    {
        image.uvRect = 
            new Rect(image.uvRect.position + new Vector2(x, y) * moveSpeed * Time.deltaTime, image.uvRect.size);
    }
}