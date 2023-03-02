using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowTrail : MonoBehaviour
{
    [SerializeField] Vector2 offset = new Vector2(.3f, -.17f);

    TrailRenderer TrlRndCaster;
    TrailRenderer TrlRndshadow;

    Transform transCaster;
    Transform transShadow;

    void Start()
    {
        transCaster = transform;
        transShadow = new GameObject().transform;
        transShadow.parent = transCaster;
        transShadow.gameObject.name = "Shadow";
        transShadow.localRotation = Quaternion.identity;

        TrlRndCaster = GetComponent<TrailRenderer>();
        TrlRndshadow = transShadow.gameObject.AddComponent<TrailRenderer>();

        
        TrlRndshadow.startColor = new Color(0, 0, 0, .5f);
        TrlRndshadow.endColor = new Color(0, 0, 0, .5f);
        TrlRndshadow.material = TrlRndCaster.material;

        TrlRndshadow.widthCurve = TrlRndCaster.widthCurve;
        TrlRndshadow.startWidth = TrlRndCaster.startWidth;
        TrlRndshadow.endWidth = TrlRndCaster.endWidth;
        TrlRndshadow.sortingLayerName = "Shadow";
    }

    void LateUpdate()
    {
        // 캐스터의 sprite renderer가 disabled라면 shadow의 sprite renderer도 disabled
        if(TrlRndCaster == null) return;

        TrlRndshadow.enabled = true;
        if(TrlRndCaster.enabled == false) TrlRndshadow.enabled = false; 

        transShadow.position = new Vector2(transCaster.position.x + offset.x,
            transCaster.position.y + offset.y);
    }
}
