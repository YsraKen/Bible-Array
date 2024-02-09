using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IntPopup), true)]
public class IntPopupDrawer : PropertyDrawer
{
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		/* var instance = PropertyDrawerUtility.GetActualObjectForSerializedProperty<IntPopup>(fieldInfo, property);
			instance?.OnEditorLoad(); */
		
		var serializedValue = property.FindPropertyRelative("value");
		var options = GetOptions(property);
		
		EditorGUI.BeginProperty(rect, label, property);
		{
			serializedValue.intValue = /* (instance.showInt || options == null)?
				EditorGUI.IntField(
					rect,
					property.displayName,
					serializedValue.intValue
				): */
				EditorGUI.Popup(
					rect,
					property.displayName,
					serializedValue.intValue,
					options
				);
		}
		EditorGUI.EndProperty();
		// instance.OnEditorUpdate();
	}
	
	string[] GetOptions(SerializedProperty property){
		var serializedOptions = property.FindPropertyRelative("options");
		int size = serializedOptions.arraySize;
		
		var options = new string[size];
		
		for(int i = 0; i < size; i++)
			options[i] = $"{i}: {serializedOptions.GetArrayElementAtIndex(i).stringValue}";
		
		return options;
	}
}