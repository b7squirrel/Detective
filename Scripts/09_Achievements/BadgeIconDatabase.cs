using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배지 id ↔ 로비 전시용 프리팹 매핑을 담는 공용 데이터.
/// BadgeDisplayManager(로비 나열)와 BadgeEarnedPopup(획득 팝업)이 이 하나의 에셋을 공유해서 참조한다.
/// </summary>
[CreateAssetMenu(fileName = "BadgeIconDatabase", menuName = "Badge/Icon Database")]
public class BadgeIconDatabase : ScriptableObject
{
    [Serializable]
    public class BadgeIconEntry
    {
        [Tooltip("AchievementSO.id와 정확히 같아야 함 (예: badge_attack_1)")]
        public string badgeId;

        [Tooltip("이미지 여러 장으로 구성돼 있어도 됨. 완성된 배지 하나를 통째로 프리팹으로 저장해서 연결.")]
        public GameObject badgePrefab;
    }

    [SerializeField] List<BadgeIconEntry> badgeIcons = new();

    Dictionary<string, GameObject> lookup;

    void BuildLookupIfNeeded()
    {
        if (lookup != null) return;

        lookup = new Dictionary<string, GameObject>();
        foreach (var entry in badgeIcons)
        {
            if (entry == null || string.IsNullOrEmpty(entry.badgeId)) continue;
            if (lookup.ContainsKey(entry.badgeId))
            {
                Debug.LogWarning($"[BadgeIconDatabase] 중복된 배지 id: {entry.badgeId}");
                continue;
            }
            lookup.Add(entry.badgeId, entry.badgePrefab);
        }
    }

    public bool TryGetPrefab(string badgeId, out GameObject prefab)
    {
        BuildLookupIfNeeded();
        return lookup.TryGetValue(badgeId, out prefab) && prefab != null;
    }
}