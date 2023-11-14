using System;
using UnityEngine;

public class OnLeavingPanel : MonoBehaviour
{
    [SerializeField] AllField allField;

    void OnDisable()
    {
        LeavePanel();
    }

    private void LeavePanel()
    {
        allField.ClearSlots();
    }
}