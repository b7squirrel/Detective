using System;
using UnityEngine;

/// <summary>
/// 젬 개수 관리: 맵의 젬 수를 추적하고 최대치 제한
/// 경험치 처리: 플레이어가 젬을 획득할 때 경험치 부여
/// 잠재 경험치: 자석 범위 내 있지만 아직 획득하지 않은 젬의 경험치 계산 (UI 표시용)
///이벤트 시스템: 젬 개수 변화를 다른 시스템(UI 등)에 알림
/// </summary>
public class GemManager : MonoBehaviour
{
    // 싱글톤 인스턴스 - 어디서든 접근 가능
    public static GemManager instance;
    
    // 맵에 동시에 존재할 수 있는 최대 젬 개수
    [SerializeField] int MaxGemNumbers;
    
    //[SerializeField] Sprite[] gemSprites; // b(파랑), g(초록), p(보라), big b, big g, big p - 사용되지 않는 코드
    
    // 현재 맵에 존재하는 젬의 개수
    int gemNumbers;
    
    // 플레이어가 아직 획득하지 않은 잠재 경험치 (자석 범위 내 젬들의 합)
    int potentialExp;
    
    // 플레이어 캐릭터 참조
    Character character;
    
    // UI 업데이트를 위한 이벤트 (젬 개수가 변경될 때 발생)
    public event Action OnGemNumberChange;
    
    [Header("Feedback")]
    [SerializeField] AudioClip gemPickup_A; // 젬 획득 사운드
    
    void Awake()
    {
        // 싱글톤 패턴 초기화
        instance = this;
        
        // 씬에서 Character 컴포넌트 찾기
        character = FindObjectOfType<Character>();
    }
    
    // 주석 처리된 메서드 - 젬의 경험치 값에 따라 스프라이트와 크기를 결정하는 기능
    // 현재는 사용되지 않음 (다른 방식으로 구현된 것으로 추정)
    //public GemProperties GetGemProperties(int _exp)
    //{
    //    GemExp gemExp = new GemExp();
    //    int gemIndex = gemExp.GetGemIndex(_exp);
    //    float size = gemIndex < 3 ? 1f : 2f; // 작은 젬(1배) vs 큰 젬(2배)
    //    // 스프라이트, 크기, 경험치를 담은 클래스 반환
    //    return new GemProperties(gemSprites[gemIndex], size, _exp);
    //}
    
    /// <summary>
    /// 플레이어에게 경험치를 부여하고 사운드 재생
    /// </summary>
    /// <param name="exp">부여할 경험치 양</param>
    public void PutExpToPlayer(int exp)
    {
        PlayGemSound();
        character.level.AddExperience(exp);
    }
    
    /// <summary>
    /// 젬 획득 사운드 재생
    /// </summary>
    public void PlayGemSound() => SoundManager.instance.Play(gemPickup_A);
    
    /// <summary>
    /// 현재 젬 개수가 최대치에 도달했는지 확인
    /// </summary>
    /// <returns>최대치 도달 여부</returns>
    public bool IsMaxGemNumber() => gemNumbers >= MaxGemNumbers;
    
    /// <summary>
    /// 젬 생성 시 호출 - 젬 카운트 증가 및 이벤트 발생
    /// </summary>
    public void IncreaseGemCount()
    {
        gemNumbers++;
        OnGemNumberChange?.Invoke(); // UI 업데이트 알림
    }
    
    /// <summary>
    /// 젬 소멸/획득 시 호출 - 젬 카운트 감소 및 이벤트 발생
    /// </summary>
    public void DecreaseGemCount()
    {
        gemNumbers--;
        OnGemNumberChange?.Invoke(); // UI 업데이트 알림
    }
    
    /// <summary>
    /// 획득 가능한 잠재 경험치가 있는지 확인
    /// </summary>
    /// <returns>잠재 경험치 존재 여부</returns>
    public bool HasPotentialExp() => potentialExp > 0;
    
    /// <summary>
    /// 자석 범위 내 젬의 경험치를 잠재 경험치에 추가
    /// </summary>
    /// <param name="exp">추가할 경험치</param>
    public void IncreasePotentialExp(int exp) => potentialExp += exp;
    
    /// <summary>
    /// 잠재 경험치 초기화 (모든 젬 획득 후)
    /// </summary>
    public void ResetPotentialExp() => potentialExp = 0;
    
    /// <summary>
    /// 현재 잠재 경험치 반환
    /// </summary>
    /// <returns>잠재 경험치 값</returns>
    public int GetPotentialExp() => potentialExp;
    
    /// <summary>
    /// 현재 맵에 존재하는 젬 개수 반환
    /// </summary>
    /// <returns>젬 개수</returns>
    public int GetGemNumbers() => gemNumbers;
}
