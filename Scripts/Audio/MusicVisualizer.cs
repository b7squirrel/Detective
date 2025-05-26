using UnityEngine;
using UnityEngine.UI; // UI Image를 조절하기 위해 필요
using DG.Tweening;
using System;

public class MusicVisualizer : MonoBehaviour
{
    [Header("Transform Settings")]
    public RectTransform spriteTransform; // UI Image의 RectTransform
    
    [Header("Audio Analysis Settings")]
    public float updateStep = 0.1f; // 오디오 샘플을 업데이트할 간격
    public int sampleDataLength = 1024; // 오디오 샘플 데이터 길이
    
    [Header("Visual Settings")]
    public float scaleMultiplier = 2f; // 스케일 증폭 계수
    public float scaleDuration = 0.1f; // 스케일 애니메이션 지속 시간
    public float minScale = 0.8f; // 최소 스케일
    public float maxScale = 3f; // 최대 스케일
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private float currentUpdateTime = 0f;
    private float[] clipSampleData;
    private AudioSource audioSource;
    private bool initDone;
    private bool isFinished;
    private Vector3 originalScale;
    private Tween currentTween;
    
    // 성능 최적화를 위한 변수들
    private float lastVolume = 0f;
    private readonly float volumeThreshold = 0.001f; // 최소 볼륨 임계값
    
    void Awake()
    {
        try
        {
            // 배열 초기화
            clipSampleData = new float[sampleDataLength];
            
            // 원본 스케일 저장
            if (spriteTransform != null)
            {
                originalScale = spriteTransform.localScale;
            }
            else
            {
                Debug.LogWarning("MusicVisualizer: spriteTransform이 설정되지 않았습니다.");
                originalScale = Vector3.one;
            }
            
            Debug.Log("MusicVisualizer 초기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"MusicVisualizer Awake 오류: {e.Message}");
            enabled = false;
        }
    }
    
    public void Init(AudioSource _audioSource)
    {
        try
        {
            if (_audioSource == null)
            {
                Debug.LogError("MusicVisualizer: AudioSource가 null입니다.");
                return;
            }
            
            audioSource = _audioSource;
            
            // AudioSource 유효성 검사
            if (audioSource.clip == null)
            {
                Debug.LogWarning("MusicVisualizer: AudioSource에 clip이 설정되지 않았습니다.");
                return;
            }
            
            // 샘플 데이터 길이 조정 (clip 길이에 맞게)
            int maxSampleLength = audioSource.clip.samples;
            if (sampleDataLength > maxSampleLength)
            {
                sampleDataLength = Mathf.Max(64, maxSampleLength / 10); // 최소 64개 샘플
                clipSampleData = new float[sampleDataLength];
                Debug.Log($"샘플 데이터 길이를 {sampleDataLength}로 조정했습니다.");
            }
            
            initDone = true;
            isFinished = false;
            
            Debug.Log($"MusicVisualizer Init 완료 - Clip: {audioSource.clip.name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"MusicVisualizer Init 오류: {e.Message}");
            initDone = false;
        }
    }
    
    public void FinishSync()
    {
        try
        {
            isFinished = true;
            
            // 현재 트윈 중단
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
            }
            
            // 원본 스케일로 복원
            if (spriteTransform != null)
            {
                currentTween = spriteTransform.DOScale(originalScale, scaleDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            }
            
            Debug.Log("MusicVisualizer 동기화 종료");
        }
        catch (Exception e)
        {
            Debug.LogError($"MusicVisualizer FinishSync 오류: {e.Message}");
        }
    }
    
    void Update()
    {
        try
        {
            // 초기화 및 상태 확인
            if (!initDone || isFinished) return;
            
            // 필수 컴포넌트 확인
            if (audioSource == null || spriteTransform == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning("MusicVisualizer: 필수 컴포넌트가 null입니다.");
                return;
            }
            
            // AudioSource 상태 확인
            if (audioSource.clip == null || !audioSource.isPlaying)
            {
                if (showDebugInfo)
                    Debug.Log("MusicVisualizer: 오디오가 재생 중이 아닙니다.");
                return;
            }
            
            // 업데이트 주기 확인
            currentUpdateTime += Time.unscaledDeltaTime;
            if (currentUpdateTime < updateStep) return;
            
            currentUpdateTime = 0f;
            
            // 오디오 데이터 분석
            AnalyzeAudioAndUpdateVisual();
        }
        catch (Exception e)
        {
            Debug.LogError($"MusicVisualizer Update 오류: {e.Message}");
            // 오류 발생 시 컴포넌트 비활성화
            enabled = false;
        }
    }
    
    void AnalyzeAudioAndUpdateVisual()
    {
        try
        {
            // 현재 재생 위치가 유효한지 확인
            if (audioSource.timeSamples >= audioSource.clip.samples)
            {
                if (showDebugInfo)
                    Debug.Log("오디오 재생이 끝에 도달했습니다.");
                return;
            }
            
            // 오디오 데이터 가져오기
            int startSample = Mathf.Max(0, audioSource.timeSamples);
            int endSample = Mathf.Min(audioSource.clip.samples - 1, startSample + sampleDataLength);
            int actualLength = endSample - startSample;
            
            if (actualLength <= 0)
            {
                if (showDebugInfo)
                    Debug.LogWarning("유효하지 않은 샘플 범위입니다.");
                return;
            }
            
            // 샘플 데이터 크기 조정
            if (clipSampleData.Length != actualLength)
            {
                clipSampleData = new float[actualLength];
            }
            
            // 오디오 데이터 추출
            audioSource.clip.GetData(clipSampleData, startSample);
            
            // 볼륨 계산
            float currentVolume = CalculateVolume();
            
            // 볼륨이 임계값보다 작으면 스킵 (성능 최적화)
            if (currentVolume < volumeThreshold)
            {
                currentVolume = 0f;
            }
            
            // 스케일 적용
            ApplyVolumeToScale(currentVolume);
            
            lastVolume = currentVolume;
            
            if (showDebugInfo)
            {
                Debug.Log($"Volume: {currentVolume:F3}, TimeSamples: {audioSource.timeSamples}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"오디오 분석 오류: {e.Message}");
        }
    }
    
    float CalculateVolume()
    {
        float totalVolume = 0f;
        
        // RMS (Root Mean Square) 방식으로 볼륨 계산 (더 정확함)
        for (int i = 0; i < clipSampleData.Length; i++)
        {
            totalVolume += clipSampleData[i] * clipSampleData[i];
        }
        
        return Mathf.Sqrt(totalVolume / clipSampleData.Length);
    }
    
    void ApplyVolumeToScale(float volume)
    {
        try
        {
            // 목표 스케일 계산
            float targetScale = 1f + volume * scaleMultiplier;
            
            // 스케일 범위 제한
            targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
            
            Vector3 targetScaleVector = originalScale * targetScale;
            
            // 현재 트윈이 있다면 중단
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
            }
            
            // DOTween을 사용하여 스프라이트 스케일 애니메이션 적용
            currentTween = spriteTransform.DOScale(targetScaleVector, scaleDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true) // timeScale 영향을 받지 않도록 설정
                .OnComplete(() => currentTween = null);
        }
        catch (Exception e)
        {
            Debug.LogError($"스케일 적용 오류: {e.Message}");
        }
    }
    
    void OnDestroy()
    {
        try
        {
            // 진행 중인 트윈 정리
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"MusicVisualizer OnDestroy 오류: {e.Message}");
        }
    }
    
    void OnDisable()
    {
        try
        {
            // 컴포넌트 비활성화 시 트윈 정리
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
            }
            
            // 원본 스케일로 복원
            if (spriteTransform != null)
            {
                spriteTransform.localScale = originalScale;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"MusicVisualizer OnDisable 오류: {e.Message}");
        }
    }
    
    // 런타임 중 설정 변경 메서드들
    public void SetScaleMultiplier(float newMultiplier)
    {
        scaleMultiplier = Mathf.Max(0f, newMultiplier);
    }
    
    public void SetUpdateStep(float newStep)
    {
        updateStep = Mathf.Max(0.01f, newStep);
    }
    
    public void SetScaleDuration(float newDuration)
    {
        scaleDuration = Mathf.Max(0.01f, newDuration);
    }
    
    // 수동으로 초기화 재시도
    public void RetryInit()
    {
        initDone = false;
        isFinished = false;
        
        if (audioSource != null)
        {
            Init(audioSource);
        }
    }
}