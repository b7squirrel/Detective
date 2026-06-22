using System.Collections.Generic;
using UnityEngine;

public class FieldUIVisibilityController : MonoBehaviour
{
    [Header("숨길 UI 목록 (Hierarchy에서 드래그)")]
    [SerializeField] private List<RectTransform> targets = new List<RectTransform>();

    // ⭐ 원래 위치 저장용
    private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();

    // 화면 밖 이동 거리 (충분히 멀리)
    private const float HIDE_OFFSET = 99999f;

    private void Start()
    {
        // ⭐ 원래 위치 미리 저장
        foreach (var target in targets)
        {
            if (target != null)
                originalPositions[target] = target.anchoredPosition;
        }

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
            if (target == null) continue;

            if (shouldHide)
            {
                // ⭐ 화면 밖으로 이동
                target.anchoredPosition = new Vector2(HIDE_OFFSET, HIDE_OFFSET);
            }
            else
            {
                // ⭐ 원래 위치로 복구
                if (originalPositions.TryGetValue(target, out Vector2 original))
                    target.anchoredPosition = original;
            }
        }
    }
}