using UnityEngine;

public class PauseCardSlot : MonoBehaviour
{
    void OnEnable()
    {
        EmptySlot();
    }

    
    
    public void UpdateWeaponCardLevel(int level)
    {

    }

    public void EmptySlot()
    {
        GetComponent<CardDisp>().EmptyCardDisplay();
    }
}