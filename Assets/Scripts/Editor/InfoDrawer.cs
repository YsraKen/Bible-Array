using UnityEngine;
// using UnityEditor;

public class InfoDrawer
{
	
}

/* [CustomPropertyDrawer(typeof(Info))]
public class InfoDrawer : PropertyDrawer
{
	static float singleLine = EditorGUIUtility.singleLineHeight;
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		var typeProperty = property.FindPropertyRelative("type");
		var type = (InfoType) typeProperty.enumValueIndex;
		
		float value = singleLine;
		
		if(type == InfoType.VerseBody || type == InfoType.CommentBody)
			value *= 2.75f;
		
		return value;
	}
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		var typeProperty = property.FindPropertyRelative("type");
		var type = (InfoType) typeProperty.enumValueIndex;
		
		var contentProperty = property.FindPropertyRelative("content");
		
		if(contentProperty.arraySize < 1)
			contentProperty.arraySize = 6;
		
		switch(type)
		{
			case InfoType.Title:
				DrawTitle(rect, contentProperty);
				break;
			
			case InfoType.VerseNumber:
				DrawNumber(rect, contentProperty);
				break;
			
			case InfoType.VerseBody:
				DrawContent(rect, contentProperty);
				break;
			
			case InfoType.CommentNumber:
				DrawCommentNumber(rect, contentProperty);
				break;
			
			case InfoType.CommentFt:
				DrawCommentFt(rect, contentProperty);
				break;
			
			case InfoType.CommentBody:
				DrawCommentBody(rect, contentProperty);
				break;
		}
	}
	
	void DrawTitle(Rect rect, SerializedProperty source)
	{
		var property = source.GetArrayElementAtIndex(0);
			property.stringValue = EditorGUI.TextField(rect, "Title", property.stringValue);
	}
	
	void DrawNumber(Rect rect, SerializedProperty source)
	{
		var property = source.GetArrayElementAtIndex(1);
			property.stringValue = EditorGUI.TextField(rect, "Number", property.stringValue);
	}
	
	void DrawContent(Rect rect, SerializedProperty source)
	{
		var labelRect = rect;
			labelRect.height = singleLine;
		
		EditorGUI.LabelField(labelRect, "Content");
		
		var property = source.GetArrayElementAtIndex(2);
		var propertyRect = rect;
			propertyRect.y += singleLine;
			propertyRect.height -= singleLine;
		
		property.stringValue = EditorGUI.TextArea(propertyRect, property.stringValue);
	}
	
	void DrawCommentNumber(Rect rect, SerializedProperty source)
	{
		var property = source.GetArrayElementAtIndex(3);
			property.stringValue = EditorGUI.TextField(rect, "Comment Number", property.stringValue);
	}
	
	void DrawCommentFt(Rect rect, SerializedProperty source)
	{
		var property = source.GetArrayElementAtIndex(4);
			property.stringValue = EditorGUI.TextField(rect, "Comment Ft", property.stringValue);
	}
	
	void DrawCommentBody(Rect rect, SerializedProperty source)
	{
		var labelRect = rect;
			labelRect.height = singleLine;
		
		EditorGUI.LabelField(labelRect, "Comment Body");
		
		var property = source.GetArrayElementAtIndex(5);
		var propertyRect = rect;
			propertyRect.y += singleLine;
			propertyRect.height -= singleLine;
		
		property.stringValue = EditorGUI.TextArea(propertyRect, property.stringValue);
	}
} */