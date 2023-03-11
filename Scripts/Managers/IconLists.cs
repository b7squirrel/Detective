using UnityEngine;

public class IconLists : MonoBehaviour
{
    public static IconLists instance;

    void Awake()
    {
        instance = this;
    }

    [SerializeField] Sprite[] classIcons;
    [SerializeField] Sprite[] currencyIcons;

    public Sprite GetClassIcon(classType classType)
    {
        return classIcons[(int)classType];
    }
    public Sprite GetCurrencyIcon(currencyType currencyType)
    {
        return currencyIcons[(int)currencyType];
    }

}
