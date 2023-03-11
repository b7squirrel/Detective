using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image itemImage;
    [SerializeField] TMPro.TextMeshProUGUI itemName;
    [SerializeField] UnityEngine.UI.Image itemClassImage;
    [SerializeField] TMPro.TextMeshProUGUI itemClass;
    
    

    public virtual void SetInfo(ItemProperty itemProperty)
    {
        if(itemProperty == null)
            return;

        itemImage.sprite = itemProperty.sprite;
        itemName.text = itemProperty.name;

        itemClassImage.sprite = IconLists.instance.GetClassIcon(itemProperty.classType);
        itemClass.text = itemProperty.classType.ToString();
    }
}
