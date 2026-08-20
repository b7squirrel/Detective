using UnityEngine;

// 동료 슬롯의 "잠금" 오버레이(자물쇠+크리스탈 아이콘+가격)에 붙이는 스크립트
// companionIndex로 자신이 몇 번 동료 슬롯을 담당하는지 지정 (0~3)
public class CompanionUnlockButton : MonoBehaviour
{
    [SerializeField] int companionIndex; // 0~3

    LaunchManager launchManager;

    public void OnClick()
    {
        if (launchManager == null) launchManager = GetComponentInParent<LaunchManager>();
        launchManager.OnCompanionLockClicked(companionIndex);
    }

    // 필요 시 코드에서 동적으로 재설정할 수 있도록 제공 (기본은 인스펙터 값 사용)
    public void SetCompanionIndex(int index)
    {
        companionIndex = index;
    }
}