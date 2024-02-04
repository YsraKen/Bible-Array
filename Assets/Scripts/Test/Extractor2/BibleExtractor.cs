using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BibleExtractor : MonoBehaviour
{
	[TextArea(10,10)]
	public string text;
	public List<TagInfo> tags = new List<TagInfo>();
	
	[Space]
	public TargetTag[] targetTags;
	public List<ExtractedInfo> extractedInfos = new List<ExtractedInfo>();
	
	public List<Verse> verses = new List<Verse>();
	
	[ContextMenu("Get Html Tag Infos")]
	void GetHtmlTagInfos()
	{
		tags.Clear();
		int textLength = text.Length;
		
		for(int i = 0; i < textLength; i++)
		{
			// Search for HTML Tag
			char c = text[i];
			if(c != '<') continue;
			
			bool isClosingTag = text[i + 1] == '/';
			if(isClosingTag) i ++;
			
			// Determine what Tag is it
			int tagTitleEndIndex = i;
			
			for(; tagTitleEndIndex < textLength; tagTitleEndIndex ++)
			{
				if(text[tagTitleEndIndex] == ' ' || text[tagTitleEndIndex] == '>')
					break;
			}
			
			int nextIndex = i + 1;
			string tagName = text.Substring(nextIndex, tagTitleEndIndex - nextIndex);
			
			// Tag a closing info
			if(isClosingTag)
			{
				for(int t = tags.Count - 1; t >= 0; t --)
				{
					var tag = tags[t];
					
					if(!tag.closed && tag.name == tagName)
					{
						int endIndex  = text.IndexOf('>', i);
						
						tag.closed = true;
						
						tag.closingIndex.x = i - 1;
						tag.closingIndex.y = (endIndex + 2) - i;
						
						tags[t] = tag;
						i = endIndex;
						
						break;
					}
				}
			}
			
			// New tag info
			else
			{
				int endIndex  = text.IndexOf('>', i);
				
				var tag = new TagInfo();
					tag.name = tagName;
					tag.openingIndex.x = i;
					tag.openingIndex.y = (endIndex + 1) - i;
					tag.value = text.Substring(i, tag.openingIndex.y);
				
				tags.Add(tag);
				i = endIndex;
			}
		}
	}
	
	[ContextMenu("Get Bible Infos")]
	void GetBibleInfos()
	{
		extractedInfos.Clear();
		
		foreach(var tag in tags)
		{
			int targetTagIndex = Array.FindIndex(targetTags, target => target.value == tag.value);
			if(targetTagIndex < 0) continue;
			
			int contentIndex = tag.openingIndex.x + tag.openingIndex.y;
			int contentLength = tag.closingIndex.x - contentIndex;
			string contentValue = text.Substring(contentIndex, contentLength);
			
			var extractedInfo = new ExtractedInfo()
			{
				name = targetTags[targetTagIndex].name,
				targetTagIndex = targetTagIndex,
				value = contentValue
			};
			
			extractedInfos.Add(extractedInfo);
		}
	}
	
	[ContextMenu("Encode Verse Infos")]
	void EncodeVerseInfos()
	{
		verses.Clear();
		
		var verseInfo = new Verse();
		int verseNumberTracker = 1;
		
		int previousTargetIndex = 0;
		
		Verse.Comment commentInfo = null;
		string commentNumberTracker = "";
		
		for(int i = 0; i < extractedInfos.Count;  i++)
		{
			int currentTargetIndex = extractedInfos[i].targetTagIndex;
			
			switch(currentTargetIndex)
			{
				case 0: // Title
					verseInfo.title += extractedInfos[i].value;
					break;
				
				case 1: // Verse [number]
					if(int.TryParse(extractedInfos[i].value, out int verseNumber))
					{
						if(verseNumber != verseNumberTracker)
						{
							verses.Add(verseInfo);
							
							verseNumberTracker = verseNumber;
							verseInfo = new Verse();
						}
					}
					break;
				
				case 2: // Verse [body]
					verseInfo.content += extractedInfos[i].value;
					break;
				
				case 3: // Lord
					i ++;
					
					if(extractedInfos.IsInsideRange(i))
					{
						string value = $"<smallcaps>{extractedInfos[i].value}</smallcaps>";
						
						if(previousTargetIndex == 0)
							verseInfo.title += value;
						
						else
							verseInfo.content += value;
					}					
					break;
				
				case 4: // Comment [number]
					string commentNumber = extractedInfos[i].value;
					
					if(commentNumber != commentNumberTracker)
					{
						commentInfo = new Verse.Comment(){ number = commentNumber };
						
						var list = (verseInfo.comments != null)? verseInfo.comments.ToList(): new List<Verse.Comment>();
							list.Add(commentInfo);
						
						verseInfo.content += $"[[COMMENT({list.Count - 1})]]";
						verseInfo.comments = list.ToArray();
						
						commentNumberTracker = commentNumber;
					}
					break;
				
				case 5:// Comment [ft]
					if(commentInfo == null)
						commentInfo = new Verse.Comment();
					
					commentInfo.content += extractedInfos[i].value;
					break;
				
				case 6:// Comment [emphasis]
					if(commentInfo == null)
						commentInfo = new Verse.Comment();
					
					commentInfo.content += $"<b>{extractedInfos[i].value}</b>";
					break;
			}
			
			previousTargetIndex = currentTargetIndex;
		}
	}
	
	[ContextMenu("Extract")]
	public void Extract()
	{
		GetHtmlTagInfos();
		GetBibleInfos();
		EncodeVerseInfos();
	}
}

[Serializable]
public struct TargetTag
{
	public string name;
	public string value;
}

[Serializable]
public struct ExtractedInfo
{
	[HideInInspector] public string name;
	[HideInInspector] public int targetTagIndex;
	
	[TextArea]
	public string value;
}