#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// CharTexts의 WeaponLocalizedName용 Drawer
[CustomPropertyDrawer(typeof(CharTexts.WeaponLocalizedName))]
public class CharWeaponLocalizedNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var internalNameProp = property.FindPropertyRelative("weaponInternalName");
        var displayNameProp = property.FindPropertyRelative("displayName");
        var synergyNameProp = property.FindPropertyRelative("synergyDisplayName");
        
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;
        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);
        
        EditorGUI.PropertyField(rect, internalNameProp);
        rect.y += lineHeight + spacing;
        
        EditorGUI.PropertyField(rect, displayNameProp);
        rect.y += lineHeight + spacing;
        
        EditorGUI.PropertyField(rect, synergyNameProp);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight * 3) + (2f * 2);
    }
}

// ItemTexts의 ItemLocalizedName용 Drawer
[CustomPropertyDrawer(typeof(ItemTexts.ItemLocalizedName))]
public class ItemLocalizedNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var internalNameProp = property.FindPropertyRelative("itemInternalName");
        var displayNameProp = property.FindPropertyRelative("displayName");
        
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;
        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);
        
        EditorGUI.PropertyField(rect, internalNameProp);
        rect.y += lineHeight + spacing;
        
        EditorGUI.PropertyField(rect, displayNameProp);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight * 2) + 2f;
    }
}
#endif