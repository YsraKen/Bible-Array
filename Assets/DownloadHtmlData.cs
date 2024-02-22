using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

public class DownloadHtmlData : MonoBehaviour
{
	public DownloadTarget[] downloadTargets;
	public List<DownloadData> downloadDatas = new List<DownloadData>();
	
	public GeneralInformation genInfo;
	public string htmlStartRead;
	
	[ProgressBar]
	public float allProgress, versionProgress, bookProgress;
	
	[TextArea, Disabled]
	public string currentUrl;
	public string currentVersionProgress;
	public string currentBookProgress;
	
	[Space, TextArea(10,10)]
	public string info;
	public int timeEstimationCheckCount = 10;
	
	[Serializable]
	public class DownloadTarget
	{
		public string version;
		
		[TextArea]
		public string url;
		public BookSelect[] books;
	}
	
	[Serializable]
	public class DownloadData
	{
		public string version;
		public List<BookDownloadData> books = new List<BookDownloadData>();
	}
	
	[Serializable]
	public class BookDownloadData
	{
		public string name;
		// public List<TextAsset> chapters = new List<TextAsset>();
	}
	
	IEnumerator Start()
	{
		Debug.Log(Application.persistentDataPath);
		Debug.Break();
		yield return null;
		
		#region Progress Tracker
		int totalNumberOfChapters = 0;
		int versionIndex = 0;
		
		foreach(var dt in downloadTargets)
		{
			foreach(int book in dt.books)
			{
				if(book >= 0)
					totalNumberOfChapters += genInfo.bookChapterVerseInfos[book].chaptersAndVerses.Length;
			}
		}
		
		yield return null;
		#endregion
		
		float downloadStartTime = Time.time;
		// float downloadDuration = 0f;
		var downloadDurations = new List<float>();
		
		foreach(var download in downloadTargets)
		{
			yield return null;
			
			string version = download.version;
			currentVersionProgress = version;
			
			var downloadData = new DownloadData(){ version = version };
				downloadDatas.Add(downloadData);
			
			#region Progress Tracker
			int totalNumberOfChapterInCurrentBook = 0;
			int bookIndex = 0;
			
			foreach(int book in download.books)
			{
				if(book >= 0)
					totalNumberOfChapterInCurrentBook += genInfo.bookChapterVerseInfos[book].chaptersAndVerses.Length;
			}
			#endregion
			
			foreach(int book in download.books)
			{
				yield return null;
				
				currentBookProgress = "";
				
				var bookDownloadData = new BookDownloadData(){};
					downloadData.books.Add(bookDownloadData);
				
				if(book < 0) continue;
				
				var bookInfo = genInfo.bookChapterVerseInfos[book];
				string bookName = book < 0? "": bookInfo.name;
				
				bookDownloadData.name = bookName;
				
				int chaptersCount = bookInfo.chaptersAndVerses.Length;
				int chapterIndex = 0;
				
				for(; chapterIndex < chaptersCount; chapterIndex ++)
				{
					yield return null;
					
					int chapterNumber = chapterIndex + 1;
					currentBookProgress = $"{bookName} {chapterNumber}";
					string url = $"{download.url}{bookName}.{chapterNumber}.{version}";
					
					if(!TryGetTextFile($"{version}/{bookName}", $"{chapterNumber}.txt", out string savePath/* , out var txt */))
					{
						currentUrl = url;
						string htmlTarget = "";
						
						yield return GetWebData(url, html => htmlTarget = html.Substring(html.IndexOf(htmlStartRead)));
						
						File.WriteAllText(savePath, htmlTarget);
					}
					
					// bookDownloadData.chapters.Add(txt);

					#region Progress Tracker
					yield return null;
					
					bookProgress = Mathf.InverseLerp(0, chaptersCount - 1, chapterIndex);
					versionProgress = Mathf.InverseLerp(0, totalNumberOfChapterInCurrentBook - 1, bookIndex ++);
					allProgress = Mathf.InverseLerp(0, totalNumberOfChapters - 1, versionIndex ++);
					
					info = $"All Progress: {(allProgress * 100).ToString("00.0")}%";
					info += $"\nVersion Progress: {(versionProgress * 100).ToString("00.0")}%";
					info += $"\nBook Progress: {(bookProgress * 100).ToString("00.0")}%";
					
					downloadDurations.Insert(0, Time.time - downloadStartTime);
					// downloadDuration = Time.time - downloadStartTime;
					downloadStartTime = Time.time;
					
					float averageDownloadDuration = 0f;
					int averageDurationDivider = 0;
					
					for(int i = 0; i < timeEstimationCheckCount && i < downloadDurations.Count; i++)
					{
						averageDownloadDuration += downloadDurations[i];
						averageDurationDivider ++;
					}
					
					averageDownloadDuration /= averageDurationDivider;
					
					int remainingCount = (totalNumberOfChapters - 1) - versionIndex;
					float estimation = averageDownloadDuration * remainingCount;
					// float estimation = downloadDuration * remainingCount;
					
					var timeSpan = TimeSpan.FromSeconds(estimation);

					string estimationTimeSpan = string.Format
					(
						"{0:D2}:{1:D2}:{2:D2}", 
						timeSpan.Hours, 
						timeSpan.Minutes, 
						timeSpan.Seconds
					);
					
					info += $"\n\nTime Estimation: {estimationTimeSpan}\n{estimation.ToString("0.0")} seconds";
					
					#endregion
				}
			}
		}
	}
	
	IEnumerator GetWebData(string url, Action<string> onLoad)
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
		{
			// Send the request and wait for a response
			yield return webRequest.SendWebRequest();

			bool connectionError = webRequest.result == UnityWebRequest.Result.ConnectionError;
			bool protocolError = webRequest.result == UnityWebRequest.Result.ProtocolError;
			
			if (connectionError || protocolError)
				Debug.LogError($"Error: {webRequest.error}");
			
			else
				onLoad(webRequest.downloadHandler.text);
		}
	}
	
	bool TryGetTextFile(string directory, string filePath, out string path/* , out TextAsset file */)
	{
		string relativeDirectory = $"BibleData/HTML/{directory}";
		directory = $"{Application.persistentDataPath}/{relativeDirectory}";
		
		if(!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		path = $"{directory}/{filePath}";
		bool exists = File.Exists(path);
		
		// string relativePath = $"Assets/{relativeDirectory}/{filePath}";
		// file = exists? AssetDatabase.LoadAssetAtPath(relativePath, typeof(TextAsset)) as TextAsset: null;
		
		return exists;
	}
}