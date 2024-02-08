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
	public Version version;
	
	public string lang => version.Language.NameCode;
	public string ver => version.NameCode;
	
	[Space, TextArea(5,5)]
	public string description;
	
	[Space]
	public Chapter[] chapters;
	
	public Chapter this[int index] => chapters[index];
	
	#if UNITY_EDITOR
	[ContextMenu("Extract Data to JSON")]
	public void ToJson()
	{
		var wrapper = new DataWrapper()
		{
			name = Name,
			fancyName = fancyName,
			nickname = nickname,
			language = version.Language.Name,
			lang = lang,
			version = version.Name,
			ver = ver,
			description = description,
			chapters = chapters
		};
		
		string json = JsonUtility.ToJson(wrapper, true);
		
		string directory = Application.persistentDataPath + $"/BibleData/{lang}/{ver}";
		string path = $"{directory}/{nickname}-{lang}-{ver}.json";
		
		if(!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		File.WriteAllText(path, json);
		Debug.Log(path, this);
	}
	
	#endif
	
	public void SetName(string name) => Name = name;
}

[Serializable]
public class Chapter
{
	public Verse[] verses;
	
	public Verse this[int index] => verses[index];
}

[Serializable]
public class Verse
{
	#if UNITY_EDITOR
	[HideInInspector] public string name;
	#endif
	
	public string title;
	public string number;
	
	[TextArea]
	public string content;
	
	public Comment[] comments;
	
	public Comment this[int index] => comments[index];
	
	[Serializable]
	public class Comment
	{
		public string number;
		
		[TextArea]
		public string content;
	}
}

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