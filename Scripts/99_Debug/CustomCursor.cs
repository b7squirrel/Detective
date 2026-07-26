#if UNITY_EDITOR
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    private static CustomCursor instance;

    // [Header("커서 텍스처")]
    // [SerializeField] private Texture2D cursorDefault;   // 기본 손가락 모양
    // [SerializeField] private Texture2D cursorPressed;   // 눌린 손가락 모양

    // [Header("핫스팟 (클릭 지점)")]
    // [SerializeField] private Vector2 hotspot = Vector2.zero;

    // private void Awake()
    // {
    //     // 씬이 바뀌어도 중복 생성되지 않도록 처리
    //     if (instance != null)
    //     {
    //         Destroy(gameObject);
    //         return;
    //     }

    //     instance = this;
    //     DontDestroyOnLoad(gameObject);
    // }

    // private void Start()
    // {
    //     SetCursor(cursorDefault);
    // }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         SetCursor(cursorPressed);
    //     }
    //     else if (Input.GetMouseButtonUp(0))
    //     {
    //         SetCursor(cursorDefault);
    //     }
    // }

    // private void SetCursor(Texture2D texture)
    // {
    //     Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
    // }

    // private void OnDisable()
    // {
    //     Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    // }
}
#endif