using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버프 아이콘 전체를 관리.
/// FieldItemEffect의 이벤트를 구독해서 아이콘 생성/제거.
/// Icons 오브젝트(Vertical Layout Group)의 자식으로 아이콘을 추가.
/// </summary>
public class BuffDisplayManager : MonoBehaviour
{
    [Header("아이콘 프리팹 (버프 타입별)")]
    [SerializeField] GameObject buffArmourPrefab;
    [SerializeField] GameObject buffAttackPrefab;
    [SerializeField] GameObject buffCoinPrefab;
    [SerializeField] GameObject buffGemPrefab;
    [SerializeField] GameObject buffInvinciblePrefab;
    [SerializeField] GameObject buffMoveSpeedPrefab;
    [SerializeField] GameObject buffTimeStopPrefab;

    [Header("아이콘이 생성될 부모 (Icons 오브젝트)")]
    [SerializeField] Transform iconsParent;

    // 현재 활성화된 아이콘들 (버프 타입 → 아이콘)
    Dictionary<FieldBuffType, BuffIconUI> activeIcons = new Dictionary<FieldBuffType, BuffIconUI>();

    void OnEnable()
    {
        FieldItemEffect.instance.OnBuffApplied += HandleBuffApplied;
        FieldItemEffect.instance.OnBuffExpired += HandleBuffExpired;
    }

    void OnDisable()
    {
        if (FieldItemEffect.instance == null) return;
        FieldItemEffect.instance.OnBuffApplied -= HandleBuffApplied;
        FieldItemEffect.instance.OnBuffExpired -= HandleBuffExpired;
    }

    void HandleBuffApplied(FieldBuffType buffType, float duration)
    {
        if (activeIcons.TryGetValue(buffType, out BuffIconUI existingIcon))
        {
            // 이미 아이콘 있음 → 타이머만 리셋
            existingIcon.ResetTimer(duration);
        }
        else
        {
            // 새 아이콘 생성
            GameObject prefab = GetPrefab(buffType);
            if (prefab == null)
            {
                Logger.LogWarning($"[BuffDisplayManager] {buffType} 프리팹이 없습니다.");
                return;
            }

            GameObject iconObj = Instantiate(prefab, iconsParent);
            BuffIconUI icon = iconObj.GetComponent<BuffIconUI>();
            if (icon == null)
            {
                Logger.LogError($"[BuffDisplayManager] {buffType} 프리팹에 BuffIconUI가 없습니다.");
                return;
            }

            icon.Init(buffType, duration);
            activeIcons[buffType] = icon;
        }
    }

    void HandleBuffExpired(FieldBuffType buffType)
    {
        if (!activeIcons.TryGetValue(buffType, out BuffIconUI icon)) return;

        Destroy(icon.gameObject);
        activeIcons.Remove(buffType);
    }

    GameObject GetPrefab(FieldBuffType buffType)
    {
        return buffType switch
        {
            FieldBuffType.SpeedBoost => buffMoveSpeedPrefab,
            FieldBuffType.DamageBoost => buffAttackPrefab,
            FieldBuffType.DoubleExp => buffGemPrefab,
            FieldBuffType.DoubleCoin => buffCoinPrefab,
            _ => null
        };
    }
}