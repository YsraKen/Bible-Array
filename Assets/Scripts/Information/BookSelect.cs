using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct BookSelect
{
	public int value;
	
	public static implicit operator int(BookSelect bs) => bs.value;
	public static implicit operator BookSelect(int value) => new BookSelect(){ value = value };
	
	public BookChapterVerseInfo Info(GeneralInformation source) => source.bookChapterVerseInfos[value];
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BookSelect))]
public class BookSelectDrawer : PropertyDrawer
{
	static GeneralInformation _genInfo;
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		if(!_genInfo)
			_genInfo = AssetDatabase.LoadAssetAtPath($"Assets/ScriptableObjects/General Information.asset", typeof(GeneralInformation)) as GeneralInformation;
		
		EditorGUI.BeginProperty(rect, label, property);
		{
			var serializedValue = property.FindPropertyRelative("value");
			
			serializedValue.intValue = EditorGUI.Popup
			(
				rect,
				property.displayName,
				serializedValue.intValue,
				_genInfo.GetBookNames()
			);
		}
		EditorGUI.EndProperty();
	}
}
#endif