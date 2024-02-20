using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadHtmlData : MonoBehaviour
{
	public DownloadTarget[] downloadTargets;
	
	public string urlIndexTag = "<BOOK.CHAPTER>";
	public string htmlStartRead;
	
	[Range(0,1)]
	public float totalProgress, bookProgress, chapterProgress;
	
	[Serializable]
	public class DownloadTarget
	{
		[TextArea]
		public string url;
		
		public Book[] books;
		
		[Space]
		public List<BookDownloadData> bookDownloadDatas = new List<BookDownloadData>();
	}
	
	[Serializable]
	public class BookDownloadData
	{
		public string name;
		public List<ChapterDownloadData> chapterDownloadDatas = new List<ChapterDownloadData>();
	}
	
	[Serializable]
	public class ChapterDownloadData
	{
		[TextArea(20, 20)]
		public string html;
	}
	
	IEnumerator Start()
	{
		int downloadIndex = 0;
		
		foreach(var download in downloadTargets)
		{	
			int bookIndex = 0;
			
			foreach(var book in download.books)
			{
				string bookName = book? book.name: "";
				var bookDownloadData = new BookDownloadData(){ name = bookName };
				
				download.bookDownloadDatas.Add(bookDownloadData);
				
				if(!book) continue;
				
				for(int i = 0; i < book.chapters.Length; i++)
				{
					var chapterDownloadData = new ChapterDownloadData();
					bookDownloadData.chapterDownloadDatas.Add(chapterDownloadData);
				
					string target = $"{bookName}.{i + 1}";						
					string url = download.url;
					
					int targetIndex = url.IndexOf(urlIndexTag);
				
					url = url.Remove(targetIndex, urlIndexTag.Length);
					url = url.Insert(targetIndex, target);
					
					yield return GetWebData(url, html => chapterDownloadData.html = html.Substring(html.IndexOf(htmlStartRead)));
					
					chapterProgress = Mathf.InverseLerp(0, book.chapters.Length - 1, i);
				}
				
				bookProgress = Mathf.InverseLerp(0, download.books.Length - 1, bookIndex);
				yield return null;
			}
			
			totalProgress = Mathf.InverseLerp(0, downloadTargets.Length - 1, downloadIndex);
			yield return null;
		}
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
}