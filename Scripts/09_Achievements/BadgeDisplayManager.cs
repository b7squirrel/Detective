using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 로비에서 받은 배지들을 훈장처럼 나열해서 보여주는 매니저.
/// content(빈 컨테이너, Horizontal/Grid Layout Group 등)를 인스펙터에서 연결.
/// 배지 id ↔ 프리팹 매핑은 BadgeIconDatabase(공용 SO)에서 가져온다.
/// 처음 보여주는 배지는 Animator의 "NewBadge" 트리거를 한 번 발동시키고,
/// 그 이후로는 자동으로 Idle 상태로 남는다 (Animator 기본 상태 = Idle 가정).
///
/// ⭐ 이미 떠 있는 배지는 건드리지 않고, 새로 받은 배지만 추가로 생성한다.
///    (전부 파괴 후 재생성하면, 여러 배지를 연달아 받았을 때 앞서 생성된
///     배지의 반짝임 애니메이션이 다음 배지 획득 시 재생성으로 인해 끊기는 문제가 있었음)
/// </summary>
public class BadgeDisplayManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] Transform content;   // 훈장들이 나열될 컨테이너 (Layout Group 붙어있는 곳)

    [Header("데이터")]
    [SerializeField] BadgeIconDatabase badgeIconDatabase;

    const string NEW_BADGE_TRIGGER = "NewBadge"; // Animator 트리거 파라미터 이름 (Animator Controller와 정확히 일치해야 함)

    // badgeId -> 생성된 아이콘 오브젝트 (이미 떠 있는지 판단하고, 강제 리셋 시 전부 지우기 위해 사용)
    Dictionary<string, GameObject> spawnedByBadgeId = new();

    void OnEnable()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAnyRewarded += OnAnyRewarded;
            Logger.Log("[BadgeDisplayManager] OnAnyRewarded 구독 성공");
        }
        else
        {
            Logger.LogWarning("[BadgeDisplayManager] OnEnable 시점에 AchievementManager.Instance가 null - 구독 실패!");
        }

        Refresh();
    }

    void OnDisable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAnyRewarded -= OnAnyRewarded;
    }

    void OnAnyRewarded(RuntimeAchievement ra)
    {
        // 배지가 아닌 일반 보상(gem/coin 등)일 때도 호출되지만,
        // 이미 떠 있는 배지는 건드리지 않으므로 매번 호출해도 가벼움
        Refresh();
    }

    /// <summary>
    /// 받은 배지 목록을 다시 확인해서, 아직 화면에 없는 배지만 추가로 생성한다.
    /// forceRebuild가 true면 기존 아이콘을 전부 지우고 처음부터 다시 그린다 (디버그 리셋용).
    /// </summary>
    public void Refresh(bool forceRebuild = false)
    {
        if (content == null)
        {
            Logger.LogWarning("[BadgeDisplayManager] content가 연결되지 않았습니다.");
            return;
        }
        if (badgeIconDatabase == null)
        {
            Logger.LogWarning("[BadgeDisplayManager] BadgeIconDatabase가 연결되지 않았습니다.");
            return;
        }
        if (AchievementManager.Instance == null) return;

        if (forceRebuild)
        {
            foreach (var go in spawnedByBadgeId.Values)
            {
                if (go != null) Destroy(go);
            }
            spawnedByBadgeId.Clear();
        }

        List<AchievementSO> earnedBadges = AchievementManager.Instance.GetEarnedBadges();
        Logger.Log($"[BadgeDisplayManager] Refresh 호출됨 - 받은 배지 수: {earnedBadges.Count}");

        foreach (var badge in earnedBadges)
        {
            if (badge == null) continue;

            // ⭐ 이미 생성돼 있는 배지는 건드리지 않고 건너뜀 (반짝임 애니메이션 보호)
            if (spawnedByBadgeId.ContainsKey(badge.id)) continue;

            if (!badgeIconDatabase.TryGetPrefab(badge.id, out var prefab))
            {
                UnityEngine.Debug.LogWarning($"[BadgeDisplayManager] 배지 id '{badge.id}'에 매칭되는 프리팹이 없습니다.");
                continue;
            }

            GameObject go = Instantiate(prefab, content);
            if (go == null)
            {
                UnityEngine.Debug.LogError($"[BadgeDisplayManager] '{badge.id}' Instantiate 실패!");
                continue;
            }

            bool isFirstTimeSeen = !AchievementManager.Instance.IsBadgeSeen(badge.id);
            if (isFirstTimeSeen)
            {
                Animator anim = go.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    StartCoroutine(PlayNewBadgeTriggerWhenReady(anim));
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[BadgeDisplayManager] '{badge.id}' 프리팹에서 Animator를 찾을 수 없습니다.");
                }
                AchievementManager.Instance.MarkBadgeSeen(badge.id);
            }

            spawnedByBadgeId.Add(badge.id, go);
        }
    }

    IEnumerator PlayNewBadgeTriggerWhenReady(Animator anim)
    {
        // 한 프레임만으로는 부족할 수 있어서, Animator가 실제로 초기화될 때까지 계속 대기
        while (anim != null && !anim.isInitialized)
        {
            yield return null;
        }

        if (anim != null)
        {
            anim.SetTrigger(NEW_BADGE_TRIGGER);
        }
    }
}