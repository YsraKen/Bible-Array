using UnityEngine;
using UnityEditor;

public static class KenAttributesUtils
{
	public static void DrawWarning(Rect rect, SerializedProperty property, System.Type type)
	{
		string name = property.displayName;
		string current = property.type;
		string _type = ObjectNames.NicifyVariableName(type.ToString());
		
		string message = "'" + name + "' (" + current + ") is not supported by " + _type	+ ".";
		var color = GUI.color;
		
		var s = new GUIStyle(EditorStyles.textField);
			s.normal.textColor = Color.red;
		
			GUI.Box(rect, message, s);
	}
}