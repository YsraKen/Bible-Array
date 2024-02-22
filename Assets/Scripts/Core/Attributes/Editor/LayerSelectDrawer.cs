using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LayerSelectAttribute))]
public class LayerSelectDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
		EditorGUIUtility.singleLineHeight;
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		EditorGUI.BeginProperty(rect, label, property);
		
		if(property.type == "int")
			property.intValue = EditorGUI.LayerField(rect, label, property.intValue);
		
		else
			KenAttributesUtils.DrawWarning(rect, property, typeof(LayerSelectAttribute));
	
		EditorGUI.EndProperty();
	}
}