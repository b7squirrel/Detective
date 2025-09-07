using UnityEngine;
using UnityEngine.UI;

public class UITrailPoint : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 시간이 지날수록 페이드 아웃
        if (image != null)
        {
            Color color = image.color;
            color.a -= Time.unscaledDeltaTime / lifetime;
            image.color = color;
        }
    }
}