﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionBar : MonoBehaviour
{
    [SerializeField] GameObject milestonePrefab;
    [SerializeField] RectTransform progressBarRect;
    [SerializeField] Slider progressBar;
    [SerializeField] GameObject slider;

    float widthUnit;
    float normalizedLengthUnit;
    float[] eventUnits;
    int eventUnitsIndex;
    List<Animator> milestoneAnims;

    public void Init(int subBossNums, int _eventNums, List<int> _actualLength)
    {
        milestoneAnims = new List<Animator>();
        widthUnit = 720f / (subBossNums + 1);
        normalizedLengthUnit = 1f / (subBossNums + 1);
        eventUnits = new float[_actualLength.Count];
        eventUnitsIndex = 0;
        for (int i = 0; i < _actualLength.Count; i++)
        {
            eventUnits[i] = 1f/(float)_actualLength[i];
        }

        for (int i = 0; i < subBossNums + 1; i++)
        {
            GameObject milestone = Instantiate(milestonePrefab, progressBarRect);
            RectTransform milestoneRect = milestone.GetComponent<RectTransform>();
            milestoneAnims.Add(milestone.GetComponent<Animator>());

            // anchoredPosition 사용
            milestoneRect.anchoredPosition = new Vector2(widthUnit * i - 360f + widthUnit, 0);

            if (i == subBossNums)
            {
                milestoneAnims[i].SetTrigger("Boss");
            }
            else
            {
                milestoneAnims[i].SetTrigger("SubBoss");
            }
        }

        progressBar.maxValue = 1;
        progressBar.value = 0;

        slider.SetActive(true);
    }

    void StartProgressionBar()
    {
        StartCoroutine(InitBarAnim());
    }

    IEnumerator InitBarAnim()
    {
        yield return new WaitForSeconds(2f);
        slider.SetActive(true);
    }

    public void UpdateProgressBar(bool _isSubBoss)
    {
        Debug.Log($"eventUnits Length = {eventUnits.Length}, event Unit Index = {eventUnitsIndex}");
        progressBar.value += normalizedLengthUnit * eventUnits[eventUnitsIndex];
        if (_isSubBoss)
        {
            //milestoneAnims[eventUnitsIndex].SetTrigger("Cleared");
            eventUnitsIndex++;
        }
    }
}