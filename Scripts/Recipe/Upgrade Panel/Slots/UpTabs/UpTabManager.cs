using UnityEngine;

public class UpTabManager : MonoBehaviour
{
    [SerializeField] UpTab[] uptabs;
    
    public void SetTab(string _cardType)
    {
        for (int i = 0; i < uptabs.Length; i++)
        {
            if (uptabs[i].GetTabType() == _cardType)
            {
                uptabs[i].SetTabActive(true);
            }
            else
            {
                uptabs[i].SetTabActive(false);
            }
        }
    }
}