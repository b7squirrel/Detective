using UnityEngine;
/// <summary>
/// 코인 개수 표시: 이번 게임에서 획득한 코인 수를 실시간으로 UI에 표시
/// 애니메이션 재생: 코인 획득 시 아이콘에 "Pop" 애니메이션 효과
/// 중복 방지: 애니메이션이 이미 재생 중일 때는 새로운 트리거를 발동하지 않음
/// 이벤트 기반: CoinManager의 OnCoinAcquired 이벤트를 구독하여 자동 업데이트
/// </summary>
public class DisplayCoinNumbers : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI coinNumbers;  // 코인 개수를 표시할 텍스트
    [SerializeField] Animator coinIconAnim;              // 코인 아이콘 애니메이터
    
    CoinManager coinManager;  // 코인 매니저 참조
    
    void Start()
    {
        // 같은 게임오브젝트의 CoinManager 가져오기
        coinManager = GetComponent<CoinManager>();
        
        // 코인 획득 이벤트에 UI 업데이트 함수 구독
        coinManager.OnCoinAcquired += UpdateCoinNumberDisp;
    }
    
    // 획득한 코인의 개수를 화면에 표시
    void UpdateCoinNumberDisp()
    {
        // 이번 세션에서 획득한 코인 수를 텍스트로 표시
        coinNumbers.text = coinManager.GetCoinNumPickedup().ToString();
        
        // Pop 애니메이션이 이미 재생 중이면 중복 재생 방지
        if (IsPopAnimPlaying())
            return;
        
        // 코인 아이콘 Pop 애니메이션 트리거
        coinIconAnim.SetTrigger("Pop");
    }
    
    // Pop 애니메이션이 현재 재생 중인지 확인
    bool IsPopAnimPlaying()
    {
        // 현재 애니메이터 상태가 "Pop"인지 확인
        if (coinIconAnim.GetCurrentAnimatorStateInfo(0).IsName("Pop"))
        {
            return true;
        }
        return false;
    }
}