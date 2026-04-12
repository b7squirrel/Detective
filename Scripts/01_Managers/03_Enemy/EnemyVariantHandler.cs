using UnityEngine;

public enum EnemyVariantType
{
    Normal,          // 1~6스테이지
    Madness,         // 7~12스테이지 - 공격속도 증가
    Helmet,          // 13~18스테이지 - 방어력 증가
    MadnessHelmet,   // 19~24스테이지 - 둘 다
    Explosive        // 25~30스테이지 - 접촉 시 폭발
}

public class EnemyVariantHandler : MonoBehaviour
{
    [Header("Overlay Objects")]
    [SerializeField] GameObject overlayMadness;   // 눈 빛나는 이미지
    [SerializeField] GameObject overlayHelmet;    // 헬멧 이미지
    [SerializeField] GameObject overlayFuse;      // 심지 이미지

    public EnemyVariantType CurrentVariant { get; private set; }

    // EnemyBase의 InitEnemy()에서 호출
    public void ApplyVariant(EnemyVariantType variantType)
    {
        CurrentVariant = variantType;

        // 모든 오버레이 초기화
        SetOverlaysOff();

        // 해당 variant의 오버레이만 ON
        switch (variantType)
        {
            case EnemyVariantType.Normal:
                // 오버레이 없음
                break;

            case EnemyVariantType.Madness:
                if (overlayMadness != null) overlayMadness.SetActive(true);
                break;

            case EnemyVariantType.Helmet:
                if (overlayHelmet != null) overlayHelmet.SetActive(true);
                break;

            case EnemyVariantType.MadnessHelmet:
                if (overlayMadness != null) overlayMadness.SetActive(true);
                if (overlayHelmet != null) overlayHelmet.SetActive(true);
                break;

            case EnemyVariantType.Explosive:
                if (overlayFuse != null) overlayFuse.SetActive(true);
                break;
        }
    }

    void SetOverlaysOff()
    {
        if (overlayMadness != null) overlayMadness.SetActive(false);
        if (overlayHelmet != null) overlayHelmet.SetActive(false);
        if (overlayFuse != null) overlayFuse.SetActive(false);
    }

    // 스테이지 번호로 variant 결정 (EnemyBase에서 호출)
    public static EnemyVariantType GetVariantForStage(int stageNumber)
    {
        int cycle = (stageNumber - 1) / 6; // 0, 1, 2, 3, 4...

        switch (cycle)
        {
            case 0: return EnemyVariantType.Normal;
            case 1: return EnemyVariantType.Madness;
            case 2: return EnemyVariantType.Helmet;
            case 3: return EnemyVariantType.MadnessHelmet;
            case 4: return EnemyVariantType.Explosive;
            default: return EnemyVariantType.Explosive; // 30스테이지 이후
        }
    }
}