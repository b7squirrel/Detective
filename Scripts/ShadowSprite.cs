using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSprite : MonoBehaviour
{
    Vector2 offset = new Vector2(.17f, -.3f);

    SpriteRenderer sprRndCaster;
    SpriteRenderer sprRndshadow;

    Transform transCaster;
    Transform transShadow;

    public bool InAir{get; set;} // shadowHeight에서 껐다가 켰다가.

    void Start()
    {
        transCaster = transform;
        transShadow = new GameObject().transform;
        transShadow.parent = transCaster;
        transShadow.gameObject.name = "Shadow";
        transShadow.localRotation = Quaternion.identity;

        sprRndCaster = GetComponent<SpriteRenderer>();
        sprRndshadow = transShadow.gameObject.AddComponent<SpriteRenderer>();

        
        sprRndshadow.color = new Color(0, 0, .2f, .7f);
        // sprRndshadow.sortingLayerName = "Shadow";
        sprRndshadow.sortingLayerName = sprRndCaster.sortingLayerName;
        sprRndshadow.sortingOrder = sprRndCaster.sortingOrder - 1;
    }

    void LateUpdate()
    {
        if (InAir)
            return;
        // 캐스터의 sprite renderer가 disabled라면 shadow의 sprite renderer도 disabled
        if(sprRndCaster == null) return;

        sprRndshadow.enabled = true;
        if(sprRndCaster.enabled == false) sprRndshadow.enabled = false; 

        transShadow.position = new Vector2(transCaster.position.x + offset.x,
            transCaster.position.y + offset.y);

        sprRndshadow.sprite = sprRndCaster.sprite;
        sprRndshadow.flipX = sprRndCaster.flipX;
        sprRndshadow.flipY = sprRndCaster.flipY;

        sprRndshadow.sortingLayerName = sprRndCaster.sortingLayerName;
        sprRndshadow.sortingOrder = sprRndCaster.sortingOrder - 1;
    }

    public void Hide()
    {
        sprRndshadow.color = new Color(sprRndshadow.color.r, sprRndshadow.color.g, sprRndshadow.color.b, 0f);
    }
    public void Show()
    {
        sprRndshadow.color = new Color(sprRndshadow.color.r, sprRndshadow.color.g, sprRndshadow.color.b, .5f);
    }
}
