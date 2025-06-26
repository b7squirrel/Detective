using UnityEngine;

/// <summary>
/// Boss에 붙여서 슬라임 드롭을 관리
/// 슬라임 드롭 오브젝트들의 신호를 받아 대표로 플레이어를 공격함. 중복 공격 막기 위함
/// </summary>
public class SlimeDropManager : MonoBehaviour
{
    [SerializeField] GameObject slimeDropPrefab; // 슬라임 점액 프리펩
    [SerializeField] int slimeDropDamage; // 슬라임 점액 데미지
    bool isTriggered; // 중첩되어 슬라임과 충돌했을 때 데미지가 너무 많이 들어가는 것을 방지
    int overrapingSlimeCount; // 슬라임 위에 있는지 체크하기 위한 플레이어와 중첩되는 슬라임 갯수

    #region 슬라임 드롭/슛
    public void DropSlimeDrop(Vector2 dropPos)
    {
        // 보스의 현재 위치에 바로 점액을 떨어트림
        GameObject slime = Instantiate(slimeDropPrefab, dropPos, Quaternion.identity);
        slime.GetComponent<ShadowHeight>().Initialize(Vector2.zero, 0f);
    }
    public void ShootSlimeDrop(Vector2 ShootPos)
    {
        // 보스의 현재 위치에서 점액을 발사
        GameObject slime = Instantiate(slimeDropPrefab, ShootPos, Quaternion.identity);
        slime.GetComponent<ShadowHeight>().Initialize(Vector2.zero, 0f);
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
    }
    public void ExitSlime()
    {
        overrapingSlimeCount--;
        if (overrapingSlimeCount <= 0) overrapingSlimeCount = 0;
        isTriggered = false;
    }
    public void Attack()
    {
        if (isTriggered == false) return;

        // 3프레임에 한 번 공격
        if (Time.frameCount % 3 == 0)
            GameManager.instance.character.TakeDamage(slimeDropDamage, EnemyType.Melee);
    }
    #endregion

}
