using System.Collections.Generic;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] GameObject[] hitEffects;
    [SerializeField] GameObject[] dieEffefcts;
    [SerializeField] GameObject[] knockbackEffects;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip[] dieSounds;

    public GameObject GetHitEffect()
    {
        GameObject hitEffect = 
            GameManager.instance.poolManager.GetMisc(hitEffects[UnityEngine.Random.Range(0, hitEffects.Length)]);
        return hitEffect;
    }
public GameObject GetDieEffect()
    {
        GameObject dieEffect =
                GameManager.instance.poolManager.GetMisc(dieEffefcts[UnityEngine.Random.Range(0, dieEffefcts.Length)]);
        return dieEffect;
    }

public GameObject GetKnockbackEffect(int _index) => knockbackEffects[_index];

    public AudioClip GetHitSound(int _index) => hitSounds[_index];

    public AudioClip GetDieSound(int _index) => dieSounds[_index];

    //void InitHitDictionary()
    //{
    //    hitE.Add(EnemyColor.yellow, hitEffects[0]);
    //    hitE.Add(EnemyColor.green, hitEffects[1]);
    //    hitE.Add(EnemyColor.red, hitEffects[2]);
    //    hitE.Add(EnemyColor.blue, hitEffects[3]);
    //    hitE.Add(EnemyColor.purple, hitEffects[4]);
    //    hitE.Add(EnemyColor.pink, hitEffects[5]);
    //}
    //void InitHitSDictionary()
    //{
    //    hitS.Add(EnemyColor.yellow, hitSounds[0]);
    //    hitS.Add(EnemyColor.green, hitSounds[1]);
    //    hitS.Add(EnemyColor.red, hitSounds[2]);
    //    hitS.Add(EnemyColor.blue, hitSounds[3]);
    //    hitS.Add(EnemyColor.purple, hitSounds[4]);
    //    hitS.Add(EnemyColor.pink, hitSounds[5]);
    //}
}
