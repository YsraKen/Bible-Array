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
}