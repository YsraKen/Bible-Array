using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TimeSpanAttribute))]
public class TimeSpanDrawer : PropertyDrawer
{
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent defaultLabel)
	{
		if(property.type != "int") return;
		
		var att = (TimeSpanAttribute) attribute;
		int multiplier = (int) att.type;
		
		var valueRect = rect;
			valueRect.width *= 0.75f;
		
		var label = att.labelOverride != null? new GUIContent(att.labelOverride): defaultLabel;
		
		float showedValue = EditorGUI.FloatField(valueRect, label, (float) property.intValue / (float) multiplier);
		
		var typeRect = rect;
			typeRect.x += valueRect.width;
			typeRect.width -= valueRect.width;
		
		att.type = (TimeSpanType) EditorGUI.EnumPopup(typeRect, att.type);
		
		property.intValue = Mathf.RoundToInt(showedValue * multiplier);
	}
}