using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PreviewImageAttribute))]
public class PreviewImageDrawer : PropertyDrawer
{
	const float maxSize = 200f;
	static float singleLineHeight = EditorGUIUtility.singleLineHeight;
	
	public static bool forceArrayPreview = false;
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return 0f;
		
		var att = (PreviewImageAttribute) attribute;
		float height = singleLineHeight;
		
		if(att.isExpanded)
			height += singleLineHeight + Mathf.Clamp(att.size, 0, maxSize);
		
		return height;
	}

	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		var att = (PreviewImageAttribute) attribute;
		var type = fieldInfo.FieldType;
			
			if(type.IsArray)
				type = type.GetElementType();
		
		EditorGUI.BeginProperty(rect, label, property);
		{
			if(isTexture() || isSprite())
			{
				var foldoutRect = new Rect(rect.x, rect.y, rect.width * 0.5f, singleLineHeight);
				
					att.isExpanded = EditorGUI.Foldout(foldoutRect, att.isExpanded, label, true);
					property.isExpanded = att.isExpanded;
				
				property.objectReferenceValue = EditorGUI.ObjectField(rect, label, property.objectReferenceValue, type, true);
				
				if(forceArrayPreview && isArray())
					DrawArrayPreview();
			}
			else
				KenAttributesUtils.DrawWarning(rect, property, typeof(PreviewImageAttribute));
			
			bool isTexture() => type.Equals(typeof(Texture)) || type.IsSubclassOf(typeof(Texture));
			bool isSprite() => type.Equals(typeof(Sprite)) || type.IsSubclassOf(typeof(Sprite));
			bool isArray() => PropertyDrawerUtility.IsArray(property, out var p);
		}
		EditorGUI.EndProperty();
		
		void DrawArrayPreview()
		{
			bool drawable = property.isExpanded && property.objectReferenceValue;
			
			if(!drawable)
				return;
			
			var txt = AssetPreview.GetAssetPreview(property.objectReferenceValue);
			
			if(txt)
			{
				var prevRect = rect;
					prevRect.y += singleLineHeight;
					prevRect.height = Mathf.Clamp(att.size, singleLineHeight, maxSize);
					prevRect.width = txt.width * (prevRect.height / txt.height);
					
					GUI.DrawTexture(prevRect, txt);
			}
		}
	}
}