using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// x생성되면 플레이어에게 달라붙음. 
/// x일정 간격으로 데미지를 주고
/// x플레이어를 느려지게 만듬
/// 사라질 때는 깜빡깜빡
/// 중첩되어 붙을 때마다 이전에 붙어 있던 슬라임을 파괴해서 한 번에 하나의 슬라임만 붙어 있도록
/// </summary>
public class SlimeSticky : MonoBehaviour
{
    Character character;
    [SerializeField] float duration;
    [SerializeField] float slowDownFactor = .3f; // 플레이어 속도 감소. .7f이면 70%
    float lifeCounter;
    [SerializeField] int damage;
    public UnityEvent onDoneEvent; // 역할이 끝나면 사라지면서 할 일들

    void OnEnable()
    {
        lifeCounter = duration;
        character = GameManager.instance.character;

        SetPlayerMoveSpeed(slowDownFactor);
    }
    void Update()
    {
        CastDamage();
        LifeCounter();
        StickToPlayer();
    }

    void StickToPlayer()
    {
        transform.position = GameManager.instance.player.transform.position;
    }

    void CastDamage()
    {
        if (Time.frameCount % 120 != 0) // 60프레임 간격으로 
            return;
        if (character == null) return;
        character.TakeDamage(damage, EnemyType.Melee);
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
        gameObject.SetActive(false);
    }

    // 유니티 이벤트에서 실행할 함수들
    public void SetPlayerMoveSpeed(float _slowDownFactor)
    {
        // 플레이어 속도 제어
        Player.instance.SetSlowDownFator(_slowDownFactor);
    }
}
