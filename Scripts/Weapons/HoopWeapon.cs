using System.Collections.Generic;
using UnityEngine;

// projectile 체력은 모두가 공유한다. 체력이 0이 되면 사라진다.
// timeToAttack으로 쿨타임이 돌면 다시 재생된다.
// sizeOfArea를 체력으로 사용하자
public class HoopWeapon : WeaponBase
{
    [SerializeField] GameObject hoopProjectile;
    [SerializeField] List<Transform> projectiles;
    [SerializeField] List<Transform> projectilesSynergy;
    [SerializeField] Transform projSpin;
    [SerializeField] Transform projSpinSynergy;
    float projectileHealth;
    bool isProjectileActive;
    //float duration;

    public override void Init(WeaponStats stats)
    {
        base.Init(stats);
        projSpin = new GameObject("Projectile Spin Board").transform;
        projSpin.position = transform.position;
        projSpin.parent = transform;

        projSpinSynergy = new GameObject("Projectile Synergy Spin Board").transform;
        projSpinSynergy.position = transform.position;
        projSpinSynergy.parent = transform;

    }
    protected override void Update()
    {
        base.Update();
        projSpin.transform.Rotate(Vector3.forward * weaponStats.projectileSpeed * Time.deltaTime);
        projSpinSynergy.transform.Rotate(Vector3.back * weaponStats.projectileSpeed * Time.deltaTime);

        // 업그레이드 되면 프로젝타일을 중단시키고 다시 시작해서 갯수를 weaponStats.numberOfAttacks에 맞춰줌
        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;
        if (numberOfProjectilesToGen == 0)
            return;
        if (isProjectileActive == false) // 프로젝타일이 활성화 되어 있을 때만 Destroy 실행
            return;

        timer = 0; // 곧바로 업그레이드 된 갯수로 업데이트 하기위해
        DestroyProjectiles();
    }

    protected override void Attack()
    {
        base.Attack();
        GenProjectile();
    }

    void GenProjectile()
    {
        if (isProjectileActive)
            return;

        Gen(projectiles, projSpin, 0f);
        if (isSynergyWeaponActivated)
        {
            Gen(projectilesSynergy, projSpinSynergy, 2f);
            // Debug.Log("시너지 Gen");
        }
    }

    // 어떤 스핀판에 붙일지, 회전 방향은 시게인지 반시계인지, 기본 4.5거리에서 얼마나 더 넗게 퍼지는지
    void Gen(List<Transform> projectiles, Transform projSpin, float distanceOffset)
    {
        // 초기화
        if (projectiles == null)
        {
            projectiles = new List<Transform>();
        }

        int numberOfProjectilesToGen = weaponStats.numberOfAttacks - projectiles.Count;
        // if (isSynergyWeaponActivated)
        //     Debug.Log("시너지 만들어낼 갯수 " + numberOfProjectilesToGen);

        // 생성
        for (int i = 0; i < numberOfProjectilesToGen; i++)
        {
            Transform hoopObject = Instantiate(hoopProjectile, projSpin.position, Quaternion.identity).transform;
            hoopObject.parent = projSpin;
            hoopObject.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Weapon";
            projectiles.Add(hoopObject);
        }

        //배치, stat 리셋
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].gameObject.SetActive(true);

            projectiles[i].localPosition = Vector3.zero;
            projectiles[i].localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * i / weaponStats.numberOfAttacks;
            projectiles[i].Rotate(rotVec);
            projectiles[i].Translate(projectiles[i].up * (distanceOffset + 2), Space.World);

            HoopProjectile hoopProjectile = projectiles[i].GetComponentInChildren<HoopProjectile>();
            hoopProjectile.Init(this);
        }
        isProjectileActive = true;
        //duration = 5f;
    }

    public void TakeDamageProjectile()
    {
        projectileHealth--;
        if(projectileHealth < 0)
        {
            DestroyProjectiles();
        }
    }
    void DestroyProjectiles()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < projectilesSynergy.Count; i++)
        {
            projectilesSynergy[i].gameObject.SetActive(false);
            
        }
        
        isProjectileActive = false;
        timer = weaponStats.timeToAttack;
        projectileHealth = weaponStats.sizeOfArea;
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        DestroyProjectiles(); // 부셔서 isProjectileActive를 false로 해줘야 다시 생성을 하게 됨
    }
}
