#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AchievementTexts.AchievementLocalizedText))]
public class AchievementLocalizedTextDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var idProp = property.FindPropertyRelative("achievementId");
        var titleProp = property.FindPropertyRelative("title");
        var descProp = property.FindPropertyRelative("description");
        
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;
        float descHeight = EditorGUI.GetPropertyHeight(descProp);
        
        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);
        
        EditorGUI.PropertyField(rect, idProp);
        rect.y += lineHeight + spacing;
        
        EditorGUI.PropertyField(rect, titleProp);
        rect.y += lineHeight + spacing;
        
        rect.height = descHeight;
        EditorGUI.PropertyField(rect, descProp);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var descProp = property.FindPropertyRelative("description");
        float descHeight = EditorGUI.GetPropertyHeight(descProp);
        return (EditorGUIUtility.singleLineHeight * 2) + descHeight + (2f * 2);
    }
}
#endif