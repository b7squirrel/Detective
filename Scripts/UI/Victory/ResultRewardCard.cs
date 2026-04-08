using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ResultRewardCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI rewardTypeLabel;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextMeshProUGUI rewardAmountLabel;
    [SerializeField] private RectTransform postit;

    [Header("Animation Settings")]
    [SerializeField] private float punchStrength = 0.3f;
    [SerializeField] private float punchDuration = 0.5f;
    [SerializeField] private int punchVibrato = 8;
    [SerializeField] private float punchElasticity = 0.5f;
    [SerializeField] private AudioClip thudSound;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        // 시작 시 숨김
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 보상 카드를 초기화하고 탕! 애니메이션과 함께 등장시킵니다.
    /// </summary>
    /// <param name="label">보상 종류 텍스트 (예: "적 처치 보상", "스테이지 클리어 보상")</param>
    /// <param name="icon">재화 아이콘 스프라이트 (골드, 크리스탈 등)</param>
    /// <param name="amount">보상 개수</param>
    /// <param name="delay">등장 지연 시간 (여러 카드를 순서대로 띄울 때 사용)</param>
    public void Initialize(string label, Sprite icon, int amount, float delay = 0f)
    {
        rewardTypeLabel.text = label;
        rewardIcon.sprite = icon;
        rewardAmountLabel.text = "x  " + amount.ToString("N0");

        // // postitd을 randomOffset만큼 z축으로 회전
        // float randomOffset = UnityEngine.Random.Range(-10f, 10f);
        // postit.localRotation = Quaternion.Euler(0f, 0f, randomOffset);

        PlayAppearAnimation(delay);
    }

    private void PlayAppearAnimation(float delay)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.AppendInterval(delay);

        // delay 끝난 직후 큰 스케일로 세팅
        seq.AppendCallback(() => transform.localScale = 2f * Vector3.one);

        // 나오는 동시에 바로 줄어들기 시작
        seq.Append(transform.DOScale(1f, 0.02f).SetEase(Ease.InQuad).SetUpdate(true));

        // 탕! 소리
        seq.AppendCallback(() =>
        {
            if (thudSound != null)
                SoundManager.instance.Play(thudSound);
        });

        seq.Play();
    }

    /// <summary>
    /// 카드를 즉시 숨기고 초기 상태로 리셋합니다. (결과창 닫을 때 호출)
    /// </summary>
    public void Hide()
    {
        DOTween.Kill(transform);
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}