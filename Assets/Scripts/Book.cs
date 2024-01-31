using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
#endif

[CreateAssetMenu]
public class Book : ScriptableObject
{
	[field: SerializeField, FormerlySerializedAs("name")]
	public string Name { get; private set; }
	
	public string fancyName;
	public string nickname;
	
	[Space]
	public string language;
	public string lang;
	public string version;
	public string ver;
	
	[Space, TextArea(5,5)]
	public string description;
	
	[Space]
	public Chapter[] chapters;
	
	#if UNITY_EDITOR
	[ContextMenu("Extract Data to JSON")]
	void ToJson()
	{
		var wrapper = new DataWrapper()
		{
			name = name,
			fancyName = fancyName,
			nickname = nickname,
			language = language,
			lang = lang,
			version = version,
			ver = ver,
			description = description,
			chapters = chapters
		};
		
		string json = JsonUtility.ToJson(wrapper, true);
		string path = Application.dataPath + $"/{nickname}-{lang}-{ver}.json";
		
		File.WriteAllText(path, json);
		AssetDatabase.Refresh();
		
		string assetPath = path.Substring(path.IndexOf("Assets"));
		var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
		
		EditorGUIUtility.PingObject(obj);
		Debug.Log(path, obj);
	}
	
	#endif
}

[Serializable]
public class Chapter
{
	public Verse[] verses;
}

[Serializable]
public class Verse
{
	public string title;
	
	[TextArea]
	public string content;
	
	public Comment[] comments;
	
	[Serializable]
	public class Comment
	{
		public string number;
		public Content[] contents;
		
		[Serializable]
		public class Content
		{
			public string ft;
			
			[TextArea]
			public string body;
		}
		
		public void DebugLog(out string output)
		{
			output = number;
			
			foreach(var content in contents)
				output += $"{content.ft} <i><b>{content.body}</b></i>";
			
			Debug.Log(output);
		}
	}
}

[Serializable]
public class Info
{
	public InfoType type;
	
	[EnumData(typeof(InfoType))]
	public string[] content;
	
	public string Content
	{
		get => content[(int) type];
		set => content[(int) type] = value;
	}
	
	public Info(InfoType type)
	{
		this.type = type;
		content = new string[6];
	}
	
	public Info(InfoType type, string content)
	{
		this.type = type;
		this.content[(int) type] = content;
	}
}

public enum InfoType { Title, VerseNumber, VerseBody, CommentNumber, CommentFt, CommentBody }

[Serializable]
public class DataWrapper
{
	public string name;
	public string fancyName;
	public string nickname;
	public string language;
	public string lang;
	public string version;
	public string ver;
	public string description;
	public Chapter[] chapters;
}