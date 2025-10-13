using System.Collections.Generic;
using UnityEngine;
using VHierarchy.Libs;

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

    List<GameObject> drops = new(); // 보스를 처치하면 드롭들도 모두 사라지도록 하기 위해
    bool isTriggered; // 중첩되어 슬라임과 충돌했을 때 데미지가 너무 많이 들어가는 것을 방지
    int overrapingObjectsCount; // 슬라임 위에 있는지 체크하기 위한 플레이어와 중첩되는 슬라임 갯수
    bool isSlowDownActivated; // slow down factor가 활성화 되어 있는지 여부
    SlimeAttackType slimeDropType = SlimeAttackType.None;
    EnemyBase bossEnemyBase; // 접근해서 OnDie액션에 DestroyAllDrop를 등록하기 위해

    #region 슬라임 드롭/슛
    public void DropObject(Vector2 dropPos)
    {
        // 보스의 현재 위치에 바로 점액을 떨어트림
        GameObject drop = Instantiate(dropPrefab, dropPos, Quaternion.identity);
        drop.GetComponentInChildren<SlimeDrop>().InitDrop();
        if (slimeDropType == SlimeAttackType.None) slimeDropType = drop.GetComponentInChildren<SlimeDrop>().GetSlimeDropType();
        AddDrop(drop);
    }
    public void DropObjectOnLanding(Vector2 dropPos)
    {
        // 보스의 현재 위치에 바로 점액을 떨어트림
        GameObject drop = Instantiate(DropOnLandingPrefab, dropPos, Quaternion.identity);
        drop.GetComponentInChildren<SlimeDrop>().InitDrop();
        if (slimeDropType == SlimeAttackType.None) slimeDropType = drop.GetComponentInChildren<SlimeDrop>().GetSlimeDropType();
        AddDrop(drop);
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
        if (Time.frameCount % 10 == 0)
            GameManager.instance.character.TakeDamage(dropDamage, EnemyType.Melee, slimeDropType);
    }
    #endregion

    #region 드롭 오브젝트 관리
    void AddDrop(GameObject drop)
    {
        drops.Add(drop);

        //드롭 생성 시점부터 보스의 enemyBase에 Destroy All Drops 등록
        GetComponent<EnemyBase>().OnDeath += DestroyAllDrops;
    }
    public void RemoveDrop(GameObject drop)
    {
        drops.Remove(drop);
    }
    public void DestroyAllDrops()
    {
        foreach (var item in drops)
        {
            if (item != null)
            {
                Destroy(item); // 또는 Object.Destroy(item)
            }
        }
        drops.Clear(); // 리스트 비우기
    }
    #endregion

    #region 플레이어 속도 제어
    void SetPlayerMoveSpeed(float _slowDownFactor)
    {
        Player.instance.SetSlowDownFator(_slowDownFactor);
    }
    #endregion
}
