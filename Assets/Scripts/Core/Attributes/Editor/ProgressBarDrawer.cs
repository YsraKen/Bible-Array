using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ProgressBarAttribute))]
public class ProgressBarDrawer : PropertyDrawer
{
	bool isEditing;
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
		EditorGUIUtility.singleLineHeight;
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		bool isInt = property.type == "int";
		bool isFloat = property.type == "float";
		
		EditorGUI.BeginProperty(rect, label, property);
		
		if(isInt || isFloat)
		{
			HandleRightClickEvents(rect);
			var attribute = (ProgressBarAttribute) this.attribute;
			
			if(isEditing)
			{
				if(isInt)
					property.intValue = EditorGUI.IntSlider(rect, label, property.intValue, 0, (int) attribute.max);
				
				else if(isFloat)
					property.floatValue = EditorGUI.Slider(rect, label, property.floatValue, 0, attribute.max);
			}
			
			else
			{
				float floatValue = isInt?
					(float) property.intValue:
					property.floatValue;
			
				EditorGUI.ProgressBar(rect, floatValue / attribute.max, property.displayName);
			}
		}
		
		else
			KenAttributesUtils.DrawWarning(rect, property, typeof(ProgressBarAttribute));
		
		EditorGUI.EndProperty();
	}
	
	void HandleRightClickEvents(Rect rect)
	{
		var e = Event.current;
		
		if (e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition))
		{
			var context = new GenericMenu();
			string label = isEditing? "Show Progress Bar": "Edit Value";
			
			context.AddItem(new GUIContent(label), false, ()=>{ isEditing = !isEditing; });
			context.ShowAsContext();
		}
	}
}