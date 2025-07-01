using UnityEngine;

public class StickyStuffManager : MonoBehaviour
{
    // stickySlime이 생성되면 1 더하기, 소멸하면 1 빼기
    // 데미지 혹은 플레이어의 속도에 영향을 미치는 factor로 사용한다
    int totalNumOfStickySlimes; // 총 sticky slime개수
    [SerializeField] float slowDownFactor = .8f; // 플레이어 속도 감소. .7f이면 70%
    [SerializeField] int damage;

    public void AddTotalStickySlimes(int num)
    {
        totalNumOfStickySlimes += num;
        if (totalNumOfStickySlimes > 0)
        {
            SetPlayerMoveSpeed();
        }
        else
        {
            totalNumOfStickySlimes = 0;
            ResetPlayerMoveSpeed();
        }
    }
    
    void Update()
    {
        CastDamage();
    }

    void CastDamage()
    {
        if (totalNumOfStickySlimes <= 0) return; // 붙어있는 슬라임이 없다면 데미지를 입히지 않는다
        if (Time.frameCount % 120 != 0) // 120프레임 간격으로 
            return;
        GameManager.instance.character.TakeDamage(damage * totalNumOfStickySlimes, EnemyType.Melee);
    }

    void SetPlayerMoveSpeed()
    {
        float currentSlowdownFactor = Mathf.Pow(slowDownFactor, totalNumOfStickySlimes);
        Player.instance.SetSlowDownFator(currentSlowdownFactor);
    }
    void ResetPlayerMoveSpeed()
    {
        Player.instance.ResetSlowDownFactor();
    }
}
