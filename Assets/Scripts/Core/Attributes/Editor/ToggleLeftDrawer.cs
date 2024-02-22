using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ToggleLeftAttribute))]
public class ToggleLeftDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
		EditorGUIUtility.singleLineHeight;
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		EditorGUI.BeginProperty(rect, label, property);
		
		if(property.type == "bool")
			property.boolValue = EditorGUI.ToggleLeft(rect, label, property.boolValue);
		
		else
			KenAttributesUtils.DrawWarning(rect, property, typeof(ToggleLeftAttribute));
	
		EditorGUI.EndProperty();
	}
}