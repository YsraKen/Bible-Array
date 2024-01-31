using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class BibleDownloader : MonoBehaviour
{
	public Book[] books;
	
	public string urlPre;
	// public string urlBook;
	public string urlPost;
	public string htmlStartRead;
	
	[EnumData(typeof(InfoType))]
	public string[] tags; // GenericPropertyJSON:{"name":"tags","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":6,"arrayType":"string","children":[{"name":"size","type":12,"val":6},{"name":"data","type":3,"val":"<span class=\"ChapterContent_heading__xBDcs\">"},{"name":"data","type":3,"val":"<span class=\"ChapterContent_label__R2PLt\">"},{"name":"data","type":3,"val":"<span class=\"ChapterContent_content__RrUqA\">"},{"name":"data","type":3,"val":"<span class=\"ChapterContent_fr__0KsID\">"},{"name":"data","type":3,"val":"<span class=\"ft\">"},{"name":"data","type":3,"val":"<span class=\"ChapterContent_fqa__Xa2yn\">"}]}]}
	public string lordTag; // <span class="ChapterContent_nd__ECPAf"><span class="ChapterContent_content__RrUqA">
	
	
	public Book currentBook;
	
	[Range(0,1)]
	public float bookProgress, allProgress;
	
	IEnumerator Start()
	{
		float startTime = Time.time;
		
		for(int i = 0; i < books.Length; i++)
		{
			allProgress = (float) i / (float) (books.Length - 1);
			
			currentBook = books[i];
			yield return Download(currentBook);
		}
		
		float duration = Time.time - startTime;
		Debug.Log($"<b>EXIT! duration: <color=yellow>'{duration}'</color></b>");
	}
	
	IEnumerator Download(Book book)
	{
		int numberOfChapters = book.chapters.Length;
		float startTime = Time.time;
		
		for(int i = 0; i < numberOfChapters; i++)
		{
			string url = $"{urlPre}{book.name}.{i + 1}{urlPost}";
			string html = "";
			
			yield return GetWebData(url, downloadData => html = downloadData);
			
			int htmlStartIndex = html.IndexOf(htmlStartRead);
			html = html.Substring(htmlStartIndex);
			
			yield return null;
			
			// Debug.Log($"{i} / {numberOfChapters}");
			
			book.chapters[i] = Extract(html);
			bookProgress = (float) i / (float) (numberOfChapters - 1);
		}
		
		float duration = Time.time - startTime;
		Debug.Log($"Done: {book.name}. duration: '{duration}'");
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(book);
		#endif
	}
	
	IEnumerator GetWebData(string url, Action<string> onLoad)
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
		{
			// Send the request and wait for a response
			yield return webRequest.SendWebRequest();

			if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError($"Error: {webRequest.error}");
			}
			else
				onLoad(webRequest.downloadHandler.text);
		}
	}
	
	Chapter Extract(string html)
	{
		var infos = new List<Info>();
		var verses = new List<Verse>();
		
		int index = 0;
		
		while(index < html.Length)
		{
			index = html.IndexOf("<span", index);
			if(index < 0) break;
			
			string tag = "";
			
			// Tag
			while(true)
			{
				char c = html[index ++];
				tag += c;
				
				if(c == '>') break;
			}
			
			// Identify Tag
			int typeIndex = Array.FindIndex(tags, t => t == tag);
			if(typeIndex < 0) continue;
			
			var info = new Info((InfoType) typeIndex);
				infos.Add(info);
			
			// Content
			while(true)
			{
				char c = html[index];
				
				if(c == '<')
				{
					#region Search for special tags "LORD" smallcaps
					
					string potentialLordTag = "";
					string closing = "</span>";
					
					if(index + closing.Length + lordTag.Length < html.Length)
						potentialLordTag = html.Substring(index + closing.Length, lordTag.Length);
					
					if(potentialLordTag == lordTag)
					{
						info.content[typeIndex] += "<smallcaps>";
						index += lordTag.Length + closing.Length;
						
						string lordContent = "";
						c = html[index ++];
						
						while(c != '<')
						{
							lordContent += c;
							c = html[index ++];
							
							// if() break;
						}
						
						info.content[typeIndex] += lordContent;
						info.content[typeIndex] += "</smallcaps>";
						
						index += closing.Length * 2;
						index -= 1;
					}
					#endregion
					
					break;
				}
				
				info.content[typeIndex] += c;
				index ++;
			}
		}
		
		string titleCache = "";
		int currentVerseIndex = 0;
		List<Verse.Comment> currentComments = new List<Verse.Comment>();
		InfoType previousType = default;
		
		for(int i = 0; i < infos.Count; i++)
		{
			var info = infos[i];
			
			switch(info.type)
			{
				case InfoType.Title:
					titleCache = info.Content;
					break;
				
				case InfoType.VerseNumber:
					if(int.TryParse(info.Content, out int number))
					{
						var verse = new Verse();
						
						if(!string.IsNullOrEmpty(titleCache))
						{
							verse.title = titleCache;
							titleCache = null;
						}
						
						verses.Add(verse);
						currentVerseIndex = verses.Count - 1;
					}
					break;
				
				case InfoType.VerseBody:
				{
					if(verses.IsInsideRange(currentVerseIndex))
					{
						var verse = verses[currentVerseIndex];
						string value = info.Content;
						// string value = previousType == InfoType.VerseBody? "\n	" + info.Content: info.Content;
						// string value = previousType == InfoType.VerseBody? " " + info.Content: info.Content;
						
						if(previousType == InfoType.VerseBody)
						{
							char first = value[0];
							
							if(first != ' ')
								value = value.Insert(0, "\n	");
						}
					
						verse.content += value;
					}
				}
				break;
				
				case InfoType.CommentNumber:
				{
					if(!verses.IsInsideRange(currentVerseIndex))
						verses.Add(new Verse());
						
					var verse = verses[currentVerseIndex];
					
					var comment = new Verse.Comment();
						comment.number = info.Content;
					
					var comments = verse.comments != null? verse.comments.ToList(): new List<Verse.Comment>();
						comments.Add(comment);
					
					verse.comments = comments.ToArray();
					verse.content += "[COMMENT[" + (comments.Count - 1) + "]]";
				}
				break;
				
				case InfoType.CommentFt:
				{
					if(!verses.IsInsideRange(currentVerseIndex))
						verses.Add(new Verse());
					
					var verse = verses[currentVerseIndex];
					
					if(verse.comments.IsNullOrEmpty())
						verse.comments = new Verse.Comment[]{ new Verse.Comment() };
				
					int commentIndex = verse.comments.Length - 1;
					var comment = verse.comments[commentIndex];
					
					var commentContent = new Verse.Comment.Content();
						commentContent.ft = info.Content;
					
					var commentContents = comment.contents != null? comment.contents.ToList(): new List<Verse.Comment.Content>();
						commentContents.Add(commentContent);
					
					comment.contents = commentContents.ToArray();
				}
				break;
				
				case InfoType.CommentBody:
				{
					var verse = verses[currentVerseIndex];
					
					int commentIndex = verse.comments.Length - 1;
					var comment = verse.comments[commentIndex];
					
					var commentContents = comment.contents != null? comment.contents.ToList(): new List<Verse.Comment.Content>();
					
					if(commentContents.Count > 0)
					{
						var commentContent = commentContents[commentContents.Count - 1];
							commentContent.body = info.Content;
					}
					else
					{
						var commentContent = new Verse.Comment.Content();
							commentContent.body = info.Content;
						
						commentContents.Add(commentContent);
					}
					
					comment.contents = commentContents.ToArray();
				}
				break;
			}
			previousType = info.type;
		}
		
		return new Chapter(){ verses = verses.ToArray() };
	}
}