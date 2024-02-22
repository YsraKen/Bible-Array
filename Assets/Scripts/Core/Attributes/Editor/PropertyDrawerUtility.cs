using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class PropertyDrawerUtility
{
	public static T GetActualObjectForSerializedProperty<T>
	(
		FieldInfo fieldInfo,
		SerializedProperty property
	){
		var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
		
		if(obj.Equals(default(T)))
			return default(T);

		T actualObject = default(T);
		
		if(obj.GetType().IsArray)
		{
			var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
			var array = (T[]) obj;
			
			if(array.Length > index)
				actualObject = array[index];
		}
		
		else actualObject = (T) obj;
		
		return actualObject;
	}
	
	public static void DrawSprite(Sprite sprite)
	{
		var txt = AssetPreview.GetAssetPreview(sprite);
		GUILayout.Label(txt);
	}
	
	public static void DrawTexture
	(
		Texture texture,
		float width = 100,
		float height = 100
	){
		GUILayout.Label("", GUILayout.Height(height), GUILayout.Width(width));
		GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
	}
	
	public static bool IsArray(SerializedProperty property, out SerializedProperty p)
	{
		p = property;
		
		string path =  property.propertyPath;
		int idot = path.IndexOf('.');
		
			if(idot == -1)
				return false;
		
		string propName = path.Substring(0, idot);
		p = property.serializedObject.FindProperty(propName);
		
		return p.isArray;
	}
} 