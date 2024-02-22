using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;

[CustomPropertyDrawer(typeof(SceneSelectAttribute))]
public class SceneSelectDrawer : PropertyDrawer
{
	enum Type{ Int, String, Other }
	Type type;
	
	string[] options;
	bool isLoaded;
	
	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if(property.serializedObject.isEditingMultipleObjects)
			return;
		
		type =
			property.type == "int"? Type.Int:
			property.type == "string"? Type.String:
			Type.Other;
		
		if(!isLoaded)
		{
			LoadOptions();
			isLoaded = true;
		}
	
		EditorGUI.BeginProperty(rect, label, property);
		{
			
			if(type == Type.Other)
				KenAttributesUtils.DrawWarning(rect, property, typeof(SceneSelectAttribute));
			
			else
			{
				var rcItems = new List<MenuItem>();
				
				if(type == Type.Int)
				{
					int selected = GetSelectedValuePopup(type, property, rect);
					
					property.intValue = selected;
					
					// rcItems.Add(new MenuItem("Select Scene Asset", ()=> SceneLoaderDrawer.PingAsset(selected)));
				}
				
				else if(type == Type.String)
				{
					var att = (SceneSelectAttribute) attribute;
					int selected = GetSelectedValuePopup(type, property, rect);
					
					if(att.isTextField)
					{
						property.stringValue = EditorGUI.TextField(rect, label, property.stringValue);
						rcItems.Add(new MenuItem("Selection List", ()=> att.isTextField = false));
					}
				
					else
					{
						property.stringValue = (selected < 0)? property.stringValue: options[selected];
						rcItems.Add(new MenuItem("Edit String Value", ()=> att.isTextField = true));
					}
					
					// rcItems.Add(new MenuItem("Select Scene Asset", ()=> SceneLoaderDrawer.PingAsset(selected)));
				}
				
				rcItems.Add(new MenuItem("Refresh", LoadOptions));
				
				var rcRect = rect;
					rcRect.width *= 0.5f;
					rcRect.x += rcRect.width;
				
				HandleRightClickEvents(rcRect, rcItems);
			}
		}
		EditorGUI.EndProperty();
	}
	
	void LoadOptions()
	{
		int count = SceneManager.sceneCountInBuildSettings;
			options = new string[count];
		
		for(int i = 0; i < count; i++)
		{
			string path = SceneUtility.GetScenePathByBuildIndex(i);
			string name = Path.GetFileNameWithoutExtension(path);
			
			options[i] = (type == Type.Int)? i + ": " + name: name;
		}
	}
	
	int GetSelectedValuePopup(Type type, SerializedProperty property, Rect rect)
	{
		int value = 0;
		
		if(type == Type.Int)
			value = property.intValue;
		
		else if(type == Type.String)
			value = Array.FindIndex(options, option => option == property.stringValue);
		
		else
			value = default(int);
		
		// value = EditorGUI.Popup(rect, property.displayName, value, options);
		DrawPopup();
		
		return value;
		
		void DrawPopup()
		{
			bool isInsideRange = (value >= 0) && (value < options.Length());
			
			if(isInsideRange)
			{
				value = EditorGUI.Popup(rect, property.displayName, value, options);
				return;
			}
			
			switch(type)
			{
				case Type.Int: property.intValue = EditorGUI.IntField(rect, property.displayName, property.intValue); break;
				case Type.String: property.stringValue = EditorGUI.TextField(rect, property.displayName, property.stringValue); break;
			}
		}
	}
	
	void HandleRightClickEvents(Rect rect, List<MenuItem> items)
	{
		var e = Event.current;
		
		if (e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition))
		{
			var context = new GenericMenu();
			
				foreach(var item in items)
					context.AddItem(new GUIContent(item.label), false, item.function);
			
			context.ShowAsContext();
		}
	}
	
	struct MenuItem
	{
		public string label;
		public MenuFunction function;
		
		public MenuItem(string _label, MenuFunction _function)
		{
			label = _label;
			function = _function;
		}
	}
}