using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(RangeMinMaxAttribute))]
public class RangeMinMaxDrawer : PropertyDrawer
{
	static float singleLineHeight = EditorGUIUtility.singleLineHeight;
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		=> property.isExpanded ? singleLineHeight * 2: singleLineHeight;
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		bool isVector2 = property.type == "Vector2";
		bool isVector2Int = property.type == "Vector2Int";
		 
		EditorGUI.BeginProperty(rect, label, property);
		{
			if(isVector2 || isVector2Int)
			{
				var attribute = this.attribute as RangeMinMaxAttribute;
				float length = GetLength(property, isVector2Int);
				
				EditorGUI.BeginProperty(rect, label, property);
				{
					if(property.isExpanded)
						GUI.Box(rect, "");
					
					var foldoutRect = rect;
						foldoutRect.width /= 2f;
						foldoutRect.height = singleLineHeight;
					
						property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, property.displayName, true);
					
					if(property.isExpanded)
						DrawSlider(property, rect, attribute.range, isVector2Int);
					
					var valueRect = rect;
						valueRect.width /= 2f;
						valueRect.x += valueRect.width;
						valueRect.height = singleLineHeight;
						
						if(isVector2)
							property.vector2Value = EditorGUI.Vector2Field(valueRect, "", property.vector2Value);
						
						else if(isVector2Int)
							property.vector2IntValue = EditorGUI.Vector2IntField(valueRect, "", property.vector2IntValue);
				}
				EditorGUI.EndProperty();
			}
			
			else
				KenAttributesUtils.DrawWarning(rect, property, typeof(RangeMinMaxAttribute));
		}
		EditorGUI.EndProperty();
	}
	
	void DrawSlider(SerializedProperty property, Rect rect, Vector2 range, bool isInt)
	{
		float yPosition = rect.y + singleLineHeight;
		var sliderRect = new Rect(rect.x, yPosition, rect.width * 0.9f, singleLineHeight);
		
		var vector2Value = (isInt)?
			(Vector2) property.vector2IntValue:
			property.vector2Value;
		
		EditorGUI.MinMaxSlider
		(
			sliderRect,
			ref vector2Value.x,
			ref vector2Value.y,
			range.x,
			range.y
		);
		
		ClampValues(ref vector2Value, range);
		
		if(isInt)
			property.vector2IntValue = new Vector2Int
			(
				(int) vector2Value.x,
				(int) vector2Value.y
			);
		
		else
			property.vector2Value = vector2Value;
		
		var lengthRect = new Rect(rect.x + sliderRect.width, yPosition, rect.width * 0.1f, singleLineHeight);
		float length = GetLength(property, isInt);
		
			EditorGUI.LabelField(lengthRect, length.ToString());
	}
	
	float GetLength(SerializedProperty property, bool isInt)
	{
		var vector2Value = isInt?
			(Vector2) property.vector2IntValue:
			property.vector2Value;
		
		float length = vector2Value.y - vector2Value.x;
		return isInt? length: Mathf.Round(length * 1000) / 1000;
	}
	
	void ClampValues(ref Vector2 value, Vector2 range)
	{
		if(value.y == 0f)
			value.y = range.y;
		
		value.x = Mathf.Clamp(value.x, range.x, value.y);
		value.y = Mathf.Clamp(value.y, value.x, range.y);
	}
}