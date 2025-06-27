using UnityEngine;

/// <summary>
/// Boss에 붙여서 슬라임 드롭을 관리
/// 슬라임 드롭 오브젝트들의 신호를 받아 대표로 플레이어를 공격함. 중복 공격 막기 위함
/// </summary>
public class SlimeDropManager : MonoBehaviour
{
    [SerializeField] GameObject slimeDropPrefab; // 슬라임 점액 프리펩
    [SerializeField] GameObject slimeDropOnLandingPrefab; // 착지할 때 나오는 슬라임 점액 프리펩
    [SerializeField] int slimeDropDamage; // 슬라임 점액 데미지
    [SerializeField] float slowDownFactor; // 플레이어가 점액 위에 있을 때 속도 저하를 위한 인자

    bool isTriggered; // 중첩되어 슬라임과 충돌했을 때 데미지가 너무 많이 들어가는 것을 방지
    int overrapingSlimeCount; // 슬라임 위에 있는지 체크하기 위한 플레이어와 중첩되는 슬라임 갯수
    bool isSlowDownActivated; // slow down factor가 활성화 되어 있는지 여부

    #region 슬라임 드롭/슛
    public void DropSlimeDrop(Vector2 dropPos)
    {
        // 보스의 현재 위치에 바로 점액을 떨어트림
        Instantiate(slimeDropPrefab, dropPos, Quaternion.identity);
    }
    public void DropSlimeDropOnLanding(Vector2 dropPos)
    {
        // 보스의 현재 위치에 바로 점액을 떨어트림
        Instantiate(slimeDropOnLandingPrefab, dropPos, Quaternion.identity);
    }
    #endregion

    #region 슬라임 데미지 관리
    void Update()
    {
        Attack();
    }
    public void EnterSlime()
    {
        overrapingSlimeCount++;
        isTriggered = true;

        if (isSlowDownActivated == false)
        {
            SetPlayerMoveSpeed(slowDownFactor);
            isSlowDownActivated = true;
        }
    }
    public void ExitSlime()
    {
        overrapingSlimeCount--;
        if (overrapingSlimeCount <= 0) overrapingSlimeCount = 0;
        {
            isTriggered = false;

            if (isSlowDownActivated == true)
            {
                SetPlayerMoveSpeed(1f);
                isSlowDownActivated = false;
            }
        }
    }
    public void Attack()
    {
        if (isTriggered == false) return;
        if (GameManager.instance.fieldItemEffect.IsStopedWithStopwatch()) return; // 스톱워치로 멈춘 상태라면 공격하지 않도록

        // 3프레임에 한 번 공격
        if (Time.frameCount % 10f == 0)
            GameManager.instance.character.TakeDamage(slimeDropDamage, EnemyType.Melee);
    }
    #endregion

    #region 플레이어 속도 제어
    void SetPlayerMoveSpeed(float _slowDownFactor)
    {
        Player.instance.SetSlowDownFator(_slowDownFactor);
    }
    #endregion
}
