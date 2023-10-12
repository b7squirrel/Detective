using System.Collections;
using UnityEngine;

// 플레이어와 닿았을 때 OnPickup()호출
public class PickUp : MonoBehaviour
{
    [SerializeField] AudioClip pickup;
    
    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void PickedUp()
    {
        StartCoroutine(PickUpCo());
    }

    IEnumerator PickUpCo()
    {
        while (true)
        {
            if ((transform.position - Player.instance.transform.position).sqrMagnitude < .04f)
            {
                HitPlayerFeedback();
                break;
            }
            yield return null;
        }
    }

    void HitPlayerFeedback()
    {
        Character character = Player.instance.GetComponent<Character>();

        GetComponent<IPickUpObject>().OnPickUp(character);

        SoundManager.instance.Play(pickup);
        gameObject.SetActive(false);
    }
}
