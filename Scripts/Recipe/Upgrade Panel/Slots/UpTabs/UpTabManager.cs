using UnityEngine;

public class UpTabManager : MonoBehaviour
{
    [SerializeField] UpTab[] uptabs;
    string currentCardType;
    UpPanelManager upPanelManager;

    void Start()
    {
        SetTab("Weapon");
        currentCardType = "Weapon";

        upPanelManager = GetComponentInParent<UpPanelManager>();
    }
    void OnEnable()
    {
        uptabs[0].SetTabActive(true);
        uptabs[1].SetTabActive(false);
        currentCardType = "Weapon";

        Debug.Log("Tab Manager Enabled");
    }
    
    public void SetTab(string _cardType)
    {
        if (currentCardType == _cardType) return;
        currentCardType = _cardType;
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

        upPanelManager.GetIntoAllField(_cardType);
    }

}