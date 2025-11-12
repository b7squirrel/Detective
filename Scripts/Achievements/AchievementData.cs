using UnityEngine;

[CreateAssetMenu(menuName ="Achievement")]
public class AchievementData : ScriptableObject
{
    public string id;                
    public string title;
    public string description;

    public int targetValue;          
    public int rewardGem;            
    
    [HideInInspector] public int currentValue;
    [HideInInspector] public bool isCompleted;
}
