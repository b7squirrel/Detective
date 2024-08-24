using UnityEngine;
using UnityEngine.UI; // UI Image�� ����� ��� �ʿ�
using DG.Tweening;

public class MusicVisualizer : MonoBehaviour
{
    public RectTransform spriteTransform; // UI Image�� RectTransform
    public float updateStep = 0.1f; // ����� ���� ������Ʈ �ֱ�
    public int sampleDataLength = 1024; // ����� ���� ������ ����
    public float scaleMultiplier = 2f; // ������ ����
    public float scaleDuration = 0.1f; // ������ �ִϸ��̼� ���� �ð�

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

            // DOTween�� ����Ͽ� ������ �ִϸ��̼� ����
            spriteTransform.DOScale(new Vector3(targetScale, targetScale, targetScale), scaleDuration)
                           .SetEase(Ease.OutQuad)
                           .SetUpdate(true); // SetUpdate(true)�� timeScale�� ������ ���� �ʵ��� ����
        }
    }
}