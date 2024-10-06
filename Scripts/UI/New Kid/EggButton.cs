using UnityEngine;

public class EggButton : MonoBehaviour
{
    [SerializeField] AudioClip pickEggSound;
    [SerializeField] AudioClip breakEggSound;
    [SerializeField] AudioClip eggClickSound;

    bool pickedEgg;

    void OnEnable()
    {
        pickedEgg = false;
    }
    public void PlayEggClickSound()
    {
        if(pickedEgg)
        {
            SoundManager.instance.Play(breakEggSound);
        }
        else
        {
            pickedEgg = true;
            SoundManager.instance.Play(pickEggSound);
        }
    }
    public void ChangeEggImage()
    {

    }
    public void GetEggStats()
    {

    }
}
