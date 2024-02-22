using UnityEngine;
using System;

[CreateAssetMenu]
public class GeneralInformation : ScriptableObject
{
	public HtmlTag[] targetTags;
	
	public Language[] allLanguages;
	public Version[] allVersions;
	
	public BookChapterVerseInfo[] bookChapterVerseInfos;
	public int maxChapterCount = 150;
	
	static string[] _allBookNames;
	
	public string[] GetBookNames()
	{
		if(_allBookNames.IsNullOrEmpty())
		{
			int length = bookChapterVerseInfos.Length;
			_allBookNames = new string[length];
			
			for(int i = 0; i < length; i++)
				_allBookNames[i] = bookChapterVerseInfos[i].name;
		}
		
		return _allBookNames;
	}
}

[Serializable]
public struct HtmlTag
{
	public string name;
	public string value;
}

[Serializable]
public struct BookChapterVerseInfo
{
	public string name;
	public int[] chaptersAndVerses;
	
	public int this[int chapterIndex] => chaptersAndVerses[chapterIndex];
}