using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TweenTest : MonoBehaviour
{
    

    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.DOAnchorPosY(-140, 5);
    }
}
