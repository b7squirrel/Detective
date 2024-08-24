using UnityEngine;
using UnityEngine.UI; // UI Image를 사용할 경우 필요
using DG.Tweening;

public class MusicVisualizer : MonoBehaviour
{
    public RectTransform spriteTransform; // UI Image의 RectTransform
    public float updateStep = 0.1f; // 오디오 샘플 업데이트 주기
    public int sampleDataLength = 1024; // 오디오 샘플 데이터 길이
    public float scaleMultiplier = 2f; // 스케일 배율
    public float scaleDuration = 0.1f; // 스케일 애니메이션 지속 시간

    private float currentUpdateTime = 0f;
    private float[] clipSampleData;
    private AudioSource audioSource;
    private bool initDone;
    private bool isFinished;

    void Awake()
    {
        clipSampleData = new float[sampleDataLength];
    }
    public void Init(AudioSource _audioSource)
    {
        audioSource = _audioSource;
        initDone = true;
    }
    public void FinishSync()
    {
        isFinished = true;
    }
    void Update()
    {
        if (initDone == false) return;
        if (isFinished) return;

        currentUpdateTime += Time.unscaledDeltaTime;
        if (currentUpdateTime >= updateStep)
        {
            currentUpdateTime = 0f;
            audioSource.clip.GetData(clipSampleData, audioSource.timeSamples);
            float currentVolume = 0f;
            foreach (var sample in clipSampleData)
            {
                currentVolume += Mathf.Abs(sample);
            }
            currentVolume /= sampleDataLength;
            float targetScale = 1 + currentVolume * scaleMultiplier;

            // DOTween을 사용하여 스케일 애니메이션 적용
            spriteTransform.DOScale(new Vector3(targetScale, targetScale, targetScale), scaleDuration)
                           .SetEase(Ease.OutQuad)
                           .SetUpdate(true); // SetUpdate(true)로 timeScale의 영향을 받지 않도록 설정
        }
    }
}