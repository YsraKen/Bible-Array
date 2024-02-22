using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TagSelectAttribute))]
public class TagSelectDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
		EditorGUIUtility.singleLineHeight;
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		EditorGUI.BeginProperty(rect, label, property);
		
		if(property.type == "string")
		{
			if(property.stringValue == "")
				property.stringValue = "Untagged";
			
			var rcRect = rect;
				rcRect.width *= 0.5f;
				rcRect.x += rcRect.width;
			
			var att = (TagSelectAttribute) attribute;
			
			if(att.isTextField)
			{
				property.stringValue = EditorGUI.TextField(rect, label, property.stringValue);
				HandleRightClickEvents(rcRect, "Selection List", ()=> att.isTextField = false);
			}
			else
			{
				property.stringValue = EditorGUI.TagField(rect, label, property.stringValue);
				HandleRightClickEvents(rcRect, "Edit String Value", ()=> att.isTextField = true);
			}
		}
		
		else
			KenAttributesUtils.DrawWarning(rect, property, typeof(TagSelectAttribute));
	
		EditorGUI.EndProperty();
	}
	
	void HandleRightClickEvents(Rect rect, string text, GenericMenu.MenuFunction function)
	{
		var e = Event.current;
		
		if (e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition))
		{
			var context = new GenericMenu();
				context.AddItem(new GUIContent(text), false, function);
			
			context.ShowAsContext();
		}
	}
}