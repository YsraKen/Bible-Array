using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestHtmlTag : MonoBehaviour
{
	public bool debugBreak = true;
	
	[TextArea(10,10)]
	public string text;
	
	public List<TagInfo> tags = new List<TagInfo>();
	
	[ContextMenu("Tae")]
	void Tae()
	{
		StartCoroutine(TaeRoutine());
	}
	
	IEnumerator TaeRoutine()
	{
		tags.Clear();
		
		int textLength = text.Length;
		
		for(int i = 0; i < textLength; i++)
		{
			yield return Log($"i: {i}");
			
			char c = text[i];
			if(c != '<') continue;
			
			yield return Log($"c: {c}");
			
			bool isClosingTag = text[i + 1] == '/';
			if(isClosingTag) i ++;
			
			if(isClosingTag)
				yield return Log($"isClosingTag: <color=red>{isClosingTag}</color>");
			else
				yield return Log($"isClosingTag: <color=yellow>{isClosingTag}</color>");
			
			
			int tagTitleEndIndex = i;
			
			for(; tagTitleEndIndex < textLength; tagTitleEndIndex ++)
			{
				if(text[tagTitleEndIndex] == ' ' || text[tagTitleEndIndex] == '>')
				{
					yield return Log($"tagTitleEndIndex: {tagTitleEndIndex}");
					break;
				}
			}
			
			int nextIndex = i + 1;
			string tagName = text.Substring(nextIndex, tagTitleEndIndex - nextIndex);
			
			yield return Log($"tagName: {tagName}");
			
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
						yield return Log($"tag closed: <color=red>{tag.value}</color>, index of: <b>[{t}]</b>");
						
						i = endIndex;
						break;
					}
				}
			}
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
				
				yield return Log($"new tag added: <b><color=green>{tag.value}</color></b>");
			}
		}
	}
	
	IEnumerator Log(string value)
	{
		Debug.Log(value);
		
		if(debugBreak)
			Debug.Break();
		
		yield return null;
	}
	
	[ContextMenu("test")]
	void Test()
	{
		tags.Clear();
		
		int textLength = text.Length;
		
		for(int i = 0; i < textLength; i++)
		{
			char c = text[i];
			if(c != '<') continue;
			
			bool isClosingTag = text[i + 1] == '/';
			if(isClosingTag) i ++;
			
			int tagTitleEndIndex = i;
			
			for(; tagTitleEndIndex < textLength; tagTitleEndIndex ++)
			{
				if(text[tagTitleEndIndex] == ' ' || text[tagTitleEndIndex] == '>')
					break;
			}
			
			int nextIndex = i + 1;
			string tagName = text.Substring(nextIndex, tagTitleEndIndex - nextIndex);
			
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
		
		GetContents();
		UpdateTxt();
	}
	
	public Text txt;
	public int highlight;
	
	void OnValidate() => UpdateTxt();
	
	void UpdateTxt()
	{
		if(!txt) return;
		
		Highlight(tags[highlight]);
		
		void Highlight(TagInfo tag)
		{
			string value = text;
			
			var opening = tag.openingIndex;
			var closing = tag.closingIndex;
			
			if(tag.closed)
			{
				string closingTarget = text.Substring(closing.x, closing.y);
				value = value.Remove(closing.x, closing.y).Insert(closing.x, $"<b><color=red>{closingTarget}</color></b>");
			
				int contentStart = opening.x + opening.y;
				int contentLength = closing.x - contentStart;
				
				// value = value.Remove(contentStart, contentLength).Insert(contentStart, $"<i><color=white>{tag.content}</color></i>");
				string contentTarget = text.Substring(contentStart, contentLength);
				value = value.Remove(contentStart, contentLength).Insert(contentStart, $"<i><color=white>{contentTarget}</color></i>");
			}
			
			string openingTarget = text.Substring(opening.x, opening.y);
			value = value.Remove(opening.x, opening.y).Insert(opening.x, $"<b><color=yellow>{openingTarget}</color></b>");
			
			txt.text = value;
		}
	}
	
	[ContextMenu("GetContents")]
	void GetContents()
	{
		/* int textLength = text.Length;
		
		for(int i = 0; i < tags.Count; i++)
		{
			var tag = tags[i];
			if(!tag.closed) continue;
			
			int startIndex = tag.openingIndex.x + tag.openingIndex.y;
			int length = tag.closingIndex.x - startIndex;
			
			tag.content = text.Substring(startIndex, length);
			
			tags[i] = tag;
		} */
	}
	
	void Update()
	{
		if(Input.GetKeyDown("q"))
		{
			highlight --;
			
			if(highlight < 0)
				highlight = tags.Count - 1;
			
			UpdateTxt();
		}
		
		if(Input.GetKeyDown("e"))
		{
			highlight ++;
			
			if(highlight >= tags.Count)
				highlight = 0;
			
			UpdateTxt();
		}
	}
}

[System.Serializable]
public struct TagInfo
{
	public string name;
	public string value;
	
	[Space]
	// X: Start Index, Y: Length
	public Vector2Int openingIndex;
	public Vector2Int closingIndex;
	
	[HideInInspector]
	public bool closed;
	
	// [Space, TextArea(5,5)]
	// public string content;
}