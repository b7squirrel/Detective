using System.Collections.Generic;
using UnityEngine;

public class FieldUIVisibilityController : MonoBehaviour
{
    [Header("숨길 UI 목록 (Hierarchy에서 드래그)")]
    [SerializeField] private List<GameObject> targets = new List<GameObject>();

    private void Start()
    {
        Apply();
    }

    public void Apply()
    {
        GameConfig config = Resources.Load<GameConfig>("GameConfig");
        if (config == null)
        {
            Debug.LogWarning("[FieldUIVisibilityController] GameConfig를 찾을 수 없습니다!");
            return;
        }

        bool shouldHide = config.hideFieldUI;

        foreach (var target in targets)
        {
            if (target != null)
                target.SetActive(!shouldHide);
        }

        // // ⭐ 커서 숨기기 (에디터 Play Mode 녹화용)
        // Cursor.visible = !shouldHide;
        // Cursor.lockState = shouldHide ? CursorLockMode.Confined : CursorLockMode.None;
    }
}