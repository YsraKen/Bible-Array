using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(EnumDataAttribute))]
public class EnumDataDrawer : PropertyDrawer
{
	SerializedProperty array;
	
	public override float GetPropertyHeight(
		SerializedProperty property,
		GUIContent label
	){
		return property.isExpanded?
			EditorGUI.GetPropertyHeight(property, label):
			EditorGUIUtility.singleLineHeight;
	}
	
	public override void OnGUI(
		Rect rect,
		SerializedProperty property,
		GUIContent label
	){
		var enumData = (EnumDataAttribute) attribute;
		string path = property.propertyPath;
		
		if(array == null)
		{
			array = property.serializedObject.FindProperty(path.Substring(0, path.LastIndexOf('.')));
			
			if(array == null)
			{
				EditorGUI.LabelField(rect, "Use EnumDataAttribute on arrays.");
				return;
			}
		}
		
		int arraySize = enumData.names.Length;
		
		if(array.arraySize != arraySize)
			array.arraySize = arraySize;
		
		int propertyElementIndex = int.Parse(path[path.Length - 2].ToString());
		label.text = enumData.names[propertyElementIndex];
		
		EditorGUI.PropertyField(rect, property, label, true);
	}
}