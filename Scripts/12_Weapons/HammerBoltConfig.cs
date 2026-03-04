using UnityEngine;

/// <summary>
/// HammerBolt의 모든 설정값을 담는 ScriptableObject.
/// Project 창에서 에셋으로 생성 후 WhipWeapon에 연결합니다.
/// 
/// 생성 방법: Project 창 우클릭 → Create → Quack Survivors → HammerBolt Config
/// </summary>
[CreateAssetMenu(fileName = "HammerBoltConfig", menuName = "Quack Survivors/HammerBolt Config")]
public class HammerBoltConfig : ScriptableObject
{
    [Header("볼트 모양")]
    [Tooltip("선을 나누는 segment 수 (많을수록 복잡한 지그재그)")]
    public int segmentCount = 20;

    [Tooltip("수직 흔들림 최대 폭")]
    public float wiggleAmount = 0.4f;

    [Tooltip("지지직 갱신 주기(초) - 작을수록 빠르게 떨림")]
    public float wiggleInterval = 0.04f;

    [Header("비주얼")]
    public float lineWidth = 0.15f;
    [Tooltip("내부 코어 색상 (밝은 흰색)")]
    public Color coreColor = new Color(1f, 1f, 1f, 1f);

    [Tooltip("외부 글로우 색상 (밝은 파랑)")]
    public Color glowColor = new Color(0.3f, 0.7f, 1f, 1f);

    [Tooltip("외부 글로우 굵기 배수 (코어 대비)")]
    public float glowWidthMultiplier = 2.5f;

    [Header("이동 효과")]
    [Tooltip("전체 길이 중 보이는 윈도우 비율 (0.35 = 35%)")]
    public float windowSize = 0.35f;

    [Tooltip("이동 구간 비율 (전체 duration 중 이동에 쓰는 비율)")]
    public float travelRatio = 0.7f;

    [Header("데미지 판정")]
    [Tooltip("볼트 경로 데미지 감지 반경 (비주얼보다 넉넉하게)")]
    public float damageCheckRadius = 0.35f;
}