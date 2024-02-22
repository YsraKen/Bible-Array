using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PasswordAttribute))]
public class PasswordDrawer : PropertyDrawer
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
			var att = (PasswordAttribute) attribute;
			
			if(att.isRevealed)
			{
				HandleRightClickEvents(rect, "Hide", ()=> att.isRevealed = false);
				property.stringValue = EditorGUI.TextField(rect, label, property.stringValue);
			}
			else
			{
				HandleRightClickEvents(rect, "Show", ()=> att.isRevealed = true);
				property.stringValue = EditorGUI.PasswordField(rect, label, property.stringValue);
			}
		}
		
		else
			KenAttributesUtils.DrawWarning(rect, property, typeof(PasswordAttribute));
	
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