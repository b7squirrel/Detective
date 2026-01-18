using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public SkillType skillType;
    public Sprite skillIcon;  // 👈 아이콘은 여기!
    public string skillName;
    [TextArea(3, 5)]
    public string description;
    
    [Header("스킬 설정")]
    public float baseCooldown = 5f;
    public float baseDuration = 3f;
    
    [Header("시각 효과")]
    public Color skillColor = Color.white;
    public GameObject skillEffectPrefab;
}