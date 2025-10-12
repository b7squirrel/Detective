using UnityEngine;

/// <summary>
/// Boss에 붙여서 슬라임 드롭을 관리
/// 슬라임 드롭 오브젝트들의 신호를 받아 대표로 플레이어를 공격함. 중복 공격 막기 위함
/// </summary>
public class SlimeDropManager : MonoBehaviour
{
    [Header("흘리고 다니는 오브젝트")]
    [SerializeField] GameObject dropPrefab; // 슬라임 점액 프리펩
    [SerializeField] GameObject DropOnLandingPrefab; // 착지할 때 나오는 슬라임 점액 프리펩
    [SerializeField] int dropDamage; // 슬라임 점액 데미지
    [SerializeField] float slowDownFactor; // 플레이어가 점액 위에 있을 때 속도 저하를 위한 인자

    bool isTriggered; // 중첩되어 슬라임과 충돌했을 때 데미지가 너무 많이 들어가는 것을 방지
    int overrapingObjectsCount; // 슬라임 위에 있는지 체크하기 위한 플레이어와 중첩되는 슬라임 갯수
    bool isSlowDownActivated; // slow down factor가 활성화 되어 있는지 여부
    SlimeAttackType slimeDropType = SlimeAttackType.None;

    #region 슬라임 드롭/슛
    public void DropObject(Vector2 dropPos)
    {
        // 보스의 현재 위치에 바로 점액을 떨어트림
        GameObject drop = Instantiate(dropPrefab, dropPos, Quaternion.identity);
        if (slimeDropType == SlimeAttackType.None) slimeDropType = drop.GetComponentInChildren<SlimeDrop>().GetSlimeDropType();
    }
    public void DropObjectOnLanding(Vector2 dropPos)
    {
        // 보스의 현재 위치에 바로 점액을 떨어트림
        Instantiate(DropOnLandingPrefab, dropPos, Quaternion.identity);
    }
    #endregion

    #region 슬라임 데미지 관리
    void Update()
    {
        Attack();
    }
    public void EnterSlime()
    {
        overrapingObjectsCount++;
        isTriggered = true;

        if (isSlowDownActivated == false)
        {
            SetPlayerMoveSpeed(slowDownFactor);
            isSlowDownActivated = true;
        }
    }
    public void ExitSlime()
    {
        overrapingObjectsCount--;
        if (overrapingObjectsCount <= 0) overrapingObjectsCount = 0;
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

        // 3프레임에 한 번 공격
        if (Time.frameCount % 3 == 0)
            GameManager.instance.character.TakeDamage(dropDamage, EnemyType.Melee, slimeDropType);
    }
    #endregion

    #region 플레이어 속도 제어
    void SetPlayerMoveSpeed(float _slowDownFactor)
    {
        Player.instance.SetSlowDownFator(_slowDownFactor);
    }
    #endregion
}
