using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DisabledAttribute))]
public class DisabledDrawer : PropertyDrawer
{
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginDisabledGroup(true);
		EditorGUI.PropertyField(rect, property, new GUIContent(property.displayName), true);
		EditorGUI.EndDisabledGroup();
	}
}