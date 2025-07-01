using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// x생성되면 플레이어에게 달라붙음. 
/// x일정 간격으로 데미지를 주고
/// x플레이어를 느려지게 만듬
/// 사라질 때는 깜빡깜빡
/// </summary>
public class SlimeSticky : MonoBehaviour
{
    [SerializeField] float duration;
    [SerializeField] float slowDownFactor = .3f; // 플레이어 속도 감소. .7f이면 70%
    float lifeCounter;
    public UnityEvent onDoneEvent; // 역할이 끝나면 사라지면서 할 일들

    void OnEnable()
    {
        lifeCounter = duration;
        GameManager.instance.stickyStuffManager.AddTotalStickySlimes(1);
    }
    void Update()
    {
        LifeCounter();
        StickToPlayer();
    }

    void StickToPlayer()
    {
        transform.position = GameManager.instance.player.transform.position;
    }
    
    void LifeCounter()
    {
        lifeCounter -= Time.deltaTime;
        if (lifeCounter < 0f)
        {
            OnProjectileDone(); // 프로젝타일의 역할이 끝남. 유니티 이벤트 실행
        }
    }

    void OnProjectileDone()
    {
        onDoneEvent?.Invoke();
        GameManager.instance.stickyStuffManager.AddTotalStickySlimes(-1);
        gameObject.SetActive(false);
    }
    // 유니티 이벤트에서 실행할 함수들
}
