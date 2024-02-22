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
	public GeneralInformation genInfo;
	public List<ExtractedInfo> extractedInfos = new List<ExtractedInfo>();
	
	[Space]
	public string bookName;
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
		
		// var otherActiveTargetTags = new List<int>();
		// int farthestTagEndIndex = 0;
		
		// bool isRecentTargetTagActive = false;
		
		// var unclosedTags = new List<Vector2Int>();
			// X = tag targeter index
			// Y = html end index
		
		foreach(var tag in tags)
		{
			int targetTagIndex = Array.FindIndex(genInfo.targetTags, target => target.value == tag.value);
			if(targetTagIndex < 0) continue;
			
			int contentIndex = tag.openingIndex.x + tag.openingIndex.y;
			int contentLength = tag.closingIndex.x - contentIndex;
			string contentValue = text.Substring(contentIndex, contentLength);
			// int htmlEndIndex = tag.closingIndex.x + tag.closingIndex.x;
			int htmlEndIndex = tag.closingIndex.x;
			string htmlEndValue = text.Substring(htmlEndIndex, tag.closingIndex.y);
			
			var extractedInfo = new ExtractedInfo()
			{
				name = genInfo.targetTags[targetTagIndex].name,
				targetTagIndex = targetTagIndex,
				value = contentValue,
				htmlEndIndex = htmlEndIndex,
				htmlEndValue = htmlEndValue
			};
			
			/* if(htmlEndIndex > farthestTagEndIndex)
			{
				for(int i = 0; i < otherActiveTargetTags.Count; i++)
					extractedInfo.isOtherTagTargetActive[otherActiveTargetTags[i]] = true;
				
				otherActiveTargetTags.Clear();
				
				extractedInfo.isOtherTagTargetActive[targetTagIndex] = true;
				otherActiveTargetTags.Add(targetTagIndex);
				
				farthestTagEndIndex = htmlEndIndex;
			}
			 */
			
			extractedInfos.Add(extractedInfo);
		}
	}
	
	[ContextMenu("Encode Verse Infos")]
	void EncodeVerseInfos()
	{
		verses.Clear();
		
		ExtractedInfo currentExtractedInfo = null;
		
		bool isPreHeader = false;
		int preHeaderHtmlEndIndex = 0;
		
		bool isCommenting = false;
		bool isCommentNumbering = false;
		int commentHtmlEndIndex = 0;
		
		bool isLordTagActive = false;
		bool isJesusTagActive = false;
		bool isItalicTagActive = false;
		
		var currentVerse = new Verse();
		
		var actions = new Action[]
		{
			chapter, preHeader, title, verseNumber, verseBody, Lord, Jesus, italic, comment, commentNumber, commentFt, commentEmphasis
		};
		
		foreach(var extractedInfo in extractedInfos)
		{
			currentExtractedInfo =  extractedInfo;
			actions[extractedInfo.targetTagIndex]();
		}
		
		verses.Add(currentVerse);
		
		void chapter()
		{
			string value = currentExtractedInfo.value;
			
			#if UNITY_EDITOR
			value = UnityEditor.ObjectNames.NicifyVariableName(value);
			#endif
			
			// Trim the digits at the end of the string value
			for(int v = value.Length - 1; v >= 0; v --)
			{
				if(value[v] == ' ')
				{
					value = value.Substring(0, v);
					break;
				}
			}
			
			bookName = value;
		}
		
		void preHeader()
		{
			// identify the 'start' and 'end' index
			
			isPreHeader = true;
			preHeaderHtmlEndIndex = currentExtractedInfo.htmlEndIndex;
			
			currentVerse.content += tagStart("PREHEAD");
		}
		
		void title()
		{
			string value = tagEncapsulate("TITLE", currentExtractedInfo.value);
			int htmlEndIndex = currentExtractedInfo.htmlEndIndex;
			
			if(isCommentNumbering)
			{
				value += tagEnd("NUM");
				isCommentNumbering = false;
			}
			
			if(isLordTagActive)
			{
				value += "</smallcaps>";
				isLordTagActive = false;
			}
			
			if(isCommenting)
			{
				// end of preheader
				if(htmlEndIndex > commentHtmlEndIndex)
				{
					value += tagEnd("COMMENT");
					isCommenting = false;
				}
			}
		
			if(isPreHeader)
			{
				// end of preheader
				if(htmlEndIndex > preHeaderHtmlEndIndex)
				{
					value += tagEnd("PREHEAD");
					isPreHeader = false;
				}
			}
			else
			{
				if(!string.IsNullOrEmpty(currentVerse.content) && currentVerse.content.Contains("MAIN"))
				{
					currentVerse.content += tagEnd("VERSE");
					currentVerse.content += tagEnd("MAIN");
					
					verses.Add(currentVerse);
					currentVerse = new Verse();
				}
				else
					currentVerse.content += tagStart("MAIN");
				
				currentVerse.name = currentExtractedInfo.value;
			}
			
			currentVerse.content += value;
		}
		
		void verseNumber()
		{
			if(isCommenting) return;
			
			string value = currentExtractedInfo.value;
			
			if(string.IsNullOrEmpty(currentVerse.number))
				currentVerse.number = value;
			
			else if(value != currentVerse.number)
			{
				currentVerse.content += tagEnd("VERSE");
				
				if(currentVerse.content.Contains("MAIN"))
					currentVerse.content += tagEnd("MAIN");
				
				verses.Add(currentVerse);
				currentVerse = new Verse();
			}
			
			currentVerse.content += tagStart("VERSE");
			currentVerse.content += tagEncapsulate("VERSE_NUM", value);
			
			currentVerse.number = value;
		}
		
		void verseBody()
		{
			int htmlEndIndex = currentExtractedInfo.htmlEndIndex;
			
			if(isCommenting)
			{
				if(htmlEndIndex > commentHtmlEndIndex)
				{
					currentVerse.content += tagEnd("COMMENT");
					isCommenting = false;
				}
			}
			
			if(isPreHeader)
			{
				// end of preheader
				if(htmlEndIndex > preHeaderHtmlEndIndex)
				{
					currentVerse.content += tagEnd("PREHEAD");
					isPreHeader = false;
				}
			}
			
			string value = currentExtractedInfo.value;
			
			if(char.IsLetterOrDigit(value[0]))
				value = value.Insert(0, "\n\t");
			
			currentVerse.content += value;
			
			if(isLordTagActive)
			{
				currentVerse.content += "</smallcaps>";
				isLordTagActive = false;
			}
			
			if(isJesusTagActive)
			{
				currentVerse.content += tagEnd("JESUS");
				isLordTagActive = false;
			}
			
			if(isItalicTagActive)
			{
				currentVerse.content += "</i>";
				isLordTagActive = false;
			}
		}
		
		void Lord()
		{
			isLordTagActive = true;
			currentVerse.content += "<smallcaps>";
		}
		
		void Jesus()
		{
			isJesusTagActive = true;
			currentVerse.content += tagStart("JESUS");
		}
		
		void italic()
		{
			isItalicTagActive = true;
			currentVerse.content += "<i>";
		}
		
		void comment()
		{
			isCommenting = true;
			commentHtmlEndIndex = currentExtractedInfo.htmlEndIndex;
			
			currentVerse.content += tagStart("COMMENT");
		}
		
		void commentNumber()
		{
			if(isPreHeader)
			{
				isCommentNumbering = true;
				currentVerse.content += tagStart("NUM");
				
				return;
			}
			
			currentVerse.content += tagEncapsulate("NUM", currentExtractedInfo.value);
		}
		
		void commentFt()
		{
			string value = currentExtractedInfo.value;
			
			if(isPreHeader && value.Contains("<span"))
				return;
			
			currentVerse.content += value;
		}
		
		void commentEmphasis()
		{
			currentVerse.content += $"<b>{currentExtractedInfo.value}</b>";
		}
		
	string tagStart(string value) => $"<{value.ToUpper()}>";
	string tagEnd(string value) => $"</{value.ToUpper()}>";
	
	string tagEncapsulate(string tag, string value) => $"{tagStart(tag)}{value}{tagEnd(tag)}";
/* 	
		var verseInfo = new Verse();
		int previousTargetIndex = 0;
		
		bool preheaderFound = false;
		bool hasTitle = false;
		
		bool commentFound = false;
		Verse.Comment commentInfo = null;
		string commentNumberTracker = "";
		int lastCommentType = 0;
		
		var unclosedInfos = new List<ExtractedInfo>();
		
		for(int i = 0; i < extractedInfos.Count;  i++)
		{
			unclosedInfos.RemoveAll(info => info.htmlEndIndex < extractedInfos[i].htmlEndIndex);
			
			int currentTargetIndex = extractedInfos[i].targetTagIndex;
			
			switch(currentTargetIndex)
			{
				case 0: // Chapter
				{
					string value = extractedInfos[i].value;
					
					for(int v = value.Length - 1; v >= 0; v --)
					{
						if(value[v] == ' ')
						{
							value = value.Substring(0, v);
							break;
						}
					}
					
					#if UNITY_EDITOR
					bookName = UnityEditor.ObjectNames.NicifyVariableName(value);
					#else
					bookName = value;
					#endif
				}
				break;
				
				case 1: // Pre-header
				{
					// if pre-header was found, the next Titles will be recorded as pre-header
					preheaderFound = true;
					
					verseInfo.content += "<PREHEAD>";
				}
				break;
				
				case 2: // Title
				{
					string value = extractedInfos[i].value;
					
					if(preheaderFound)
					{
						verseInfo.content += value + "</PREHEAD>";
						preheaderFound = false;
					}
					else if(commentFound)
					{
						switch(lastCommentType)
						{
							case 8: onCommentNumber(); break;
							case 9: onCommentFt("", " "); break;
							case 10: onCommentEmphasis(); break;
						}
					}
					else
						verseInfo.title += value;
				}
				break;
				
				case 3: // Verse [number]
				{
					string value = extractedInfos[i].value;
					
					commentFound = value == "#";
					if(commentFound) break;
					
					if(!string.IsNullOrEmpty(verseInfo.number))
					{
						#if UNITY_EDITOR
						verseInfo.name = $"{verseInfo.number}: {verseInfo.title}";
						#endif
						
						verses.Add(verseInfo);
						verseInfo = new Verse();
					}
					
					verseInfo.number = value;
				
					#region Combined Verses
					
					// Some verses are combined into 1 section
						// In case of 'Genesis 1:17-18' ASND
					
					if(value.Contains('-'))
					{
						var numbers = new List<string>();
						string number = "";
						
						foreach(char c in value)
						{
							if(c == '-')
							{
								numbers.Add(number);
								number = "";
								
								continue;
							}
							
							number += c;
						}
						
						numbers.Add(number);
						number = "";
						
						int max = int.Parse(numbers[numbers.Count - 1]);
						int min = int.Parse(numbers[0]);
						
						for(int n = 0; n < (max - min); n++)
							verses.Add(verseInfo);
					}
					
					#endregion
				}
				break;
				
				case 4: // Verse [body]
				{
					// unique cases, title phase always end ones the verse body starts
					if(!hasTitle)
						hasTitle = true;
					
					string value = extractedInfos[i].value;
					
					if(char.IsLetterOrDigit(value[0]))
						value = value.Insert(0, "\n\t");
					
					verseInfo.content += value;
				}
				break;
				
				case 5: // Lord
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
				
				case 6: // Jesus
					i ++;
					
					if(extractedInfos.IsInsideRange(i))
					{
						string value = $"<JESUS>{extractedInfos[i].value}</JESUS>";
						
						if(previousTargetIndex == 0)
							verseInfo.title += value;
						
						else
							verseInfo.content += value;
					}	
					break;
				
				case 7: // italic
					i ++;
					
					if(extractedInfos.IsInsideRange(i))
					{
						string value = $"<i>{extractedInfos[i].value}</i>";
						
						if(previousTargetIndex == 0)
						{
							verseInfo.title += value;
						}
						
						else
							verseInfo.content += value;
					}	
					
					break;
				
				case 8: // Comment [number]
				{
					if(hasTitle)
						onCommentNumber();
					
					lastCommentType = 8;
				}
				break;
				
				void onCommentNumber()
				{
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
				}
				
				case 9:// Comment [ft]
				{
					if(hasTitle)
						onCommentFt();
					
					lastCommentType = 9;
				}
				break;
				
				void onCommentFt(string pre = "", string post = "")
				{
					if(commentInfo == null)
						commentInfo = new Verse.Comment();
					
					commentInfo.content += $"{pre}{extractedInfos[i].value}{post}";
				}
				
				case 10:// Comment [emphasis]
				{
					if(hasTitle)
						onCommentEmphasis();
					
					lastCommentType = 10;
				}
				break;
				
				void onCommentEmphasis(string pre = "", string post = "")
				{
					if(commentInfo == null)
						commentInfo = new Verse.Comment();
					
					commentInfo.content += $"<b>{pre}{extractedInfos[i].value}{post}</b>";
				}
			}
			
			previousTargetIndex = currentTargetIndex;
			unclosedInfos.Add(extractedInfos[i]);
		}
		
		#if UNITY_EDITOR
		verseInfo.name = $"{verseInfo.number}: {verseInfo.title}";
		#endif
		
		verses.Add(verseInfo); */
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
public class ExtractedInfo
{
	[HideInInspector] public string name;
	[HideInInspector] public int targetTagIndex;
	
	[TextArea]
	public string value;
	
	public int htmlEndIndex;
	public string htmlEndValue;
}