#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameTexts.WeaponLocalizedName))]
public class WeaponLocalizedNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var internalNameProp = property.FindPropertyRelative("weaponInternalName");
        var displayNameProp = property.FindPropertyRelative("displayName");
        var synergyNameProp = property.FindPropertyRelative("synergyDisplayName");
        
        EditorGUI.PropertyField(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            internalNameProp
        );
        EditorGUI.PropertyField(
            new Rect(position.x, position.y + 20, position.width, EditorGUIUtility.singleLineHeight),
            displayNameProp
        );
        EditorGUI.PropertyField(
            new Rect(position.x, position.y + 40, position.width, EditorGUIUtility.singleLineHeight),
            synergyNameProp
        );
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + 10;
    }
}
#endif