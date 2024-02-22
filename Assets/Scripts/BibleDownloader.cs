using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BibleDownloader : MonoBehaviour
{
	public BookSelect[] books;
	
	// public string urlPre;
	// public string urlPost;
	// public string htmlStartRead;
	
	[Space]
	public Language language;
	public Version version;
	
	public string currentBook;
	public int currentChapter;
	
	public BibleExtractor extractor;
	public GeneralInformation genInfo;
	
	[Range(0,1)]
	public float bookProgress, allProgress;
	
	public float StartTime { get; private set; }
	public float BookStartTime { get; private set; }
	public bool IsDone { get; private set; }
	
	#region UI
	
	private static Text _urlTxt;
	private static GameObject _graphTemplate;
	
	public static float longestTime;
	public static float graphMultiplierNormalized { get; set; } = 1f;
	
	public static List<GraphInstance> graphInstances = new List<GraphInstance>();
	
	public class GraphInstance
	{
		public Image img;
		public float sec;
		
		public GraphInstance(Image img, float sec)
		{
			this.img = img;
			this.sec = sec;
		}
	}
	
	#endregion
	
	void Awake()
	{
		if(!_urlTxt)
			_urlTxt = GameObject.FindWithTag("Download URL").GetComponent<Text>();
		
		if(!_graphTemplate)
			_graphTemplate = GameObject.FindWithTag("Download Graph");
	}
	
	IEnumerator Start()
	{
		IsDone = false;
		StartTime = Time.time;
		
		for(int i = 0; i < books.Length; i++)
		{
			yield return null;
			
			allProgress = (float) i / (float) (books.Length - 1);
			
			if(books[i] < 0) continue;
			
			var info = books[i].Info(genInfo);
			currentBook = info.name;
			
			yield return Download(info);
		}
		
		// var version = books[0].version;
			// version.SetBooks(GetArrangedBooks());
		
		yield return null;
		
		float duration = Time.time - StartTime;
		Debug.Log($"<b>EXIT! duration: <color=green>'{duration} seconds'</color></b>");
		
		IsDone = true;
	}
	
	// IEnumerator Download(Book book)
	IEnumerator Download(BookChapterVerseInfo bookInfo)
	{
		int numberOfChapters = bookInfo.chaptersAndVerses.Length;
		BookStartTime = Time.time;
		
		string langCode = language.NameCode;
		string verCode = version.NameCode;
		
		string directory = $"{Application.persistentDataPath}/BibleData/JSON/{langCode}/{verCode}/{bookInfo.name}";
		
		if(!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		for(int i = 0; i < numberOfChapters; i++)
		{
			yield return null;
			
			currentChapter = i + 1;
			bookProgress = (float) i / (float) (numberOfChapters - 1);
			
			string chapterInfoPath = $"{directory}/{langCode}-{verCode}-{bookInfo.name}-{i}.json";
			
			if(_urlTxt)
				_urlTxt.text = chapterInfoPath;
			
			if(File.Exists(chapterInfoPath))
				continue;
			
			// string url = $"{urlPre}{book.name}.{i + 1}{urlPost}";
			string html = "";			
			
			// yield return GetWebData(url, downloadData => html = downloadData);
			GetHtmlFile($"{version.NameCode}/{bookInfo.name}", $"{i + 1}.txt", loadedData => html = loadedData);
			
			// int htmlStartIndex = html.IndexOf(htmlStartRead);
			// html = html.Substring(htmlStartIndex);
			
			// yield return null;
			
			extractor.text = html;
			extractor.Extract();
			
			// yield return null;
			
			var chapter = new Chapter(){ verses = extractor.verses.ToArray() };
			// book.chapters[i] = chapter;
			// bookJsonInfo.chapters[i] = chapter;
			
			File.WriteAllText(chapterInfoPath, JsonUtility.ToJson(chapter, true));
			
			// yield return null;
			
		}
		
		// yield return null;
		// book.SetName(extractor.bookName);
		
		string bookInfoPath = $"{directory}/bookInfo.json";
		
		if(!File.Exists(bookInfoPath))
		{
			var bookJsonInfo = new BookJsonInfo()
			{
				name = extractor.bookName,
				nickname = bookInfo.name.ToLower(),
				language = language.GetIndex(),
				version = version.GetIndex()
			};
			
			
			File.WriteAllText(bookInfoPath, JsonUtility.ToJson(bookJsonInfo, true));
			// Debug.Log(path, this);
		}
		
		// yield return null;
		
		float duration = Time.time - BookStartTime;
		Debug.Log($"<color=lime>Done:</color> <b>{version.NameCode}: <color=cyan>{bookInfo.name}</color></b>. duration: <color=yellow>'{duration.ToString("0.00")} seconds'</color>");
		
		// #if UNITY_EDITOR
		// EditorUtility.SetDirty(book);
		// #endif
		
		if(_graphTemplate)
			UpdateGraphUI(duration, bookInfo);
		
		// yield return null;
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
	
	void GetHtmlFile(string directory, string filePath, Action<string> onLoad)
	{
		string path = $"{Application.persistentDataPath}/BibleData/HTML/{directory}/{filePath}";
		// string relativePath = $"Assets/{relativeDirectory}/{filePath}.txt";
		
		// var file = AssetDatabase.LoadAssetAtPath(relativePath, typeof(TextAsset)) as TextAsset;
		var file = File.ReadAllText(path);
		
		// Debug.Log(relativePath, file);
		// Debug.Break();
		
		onLoad(file);
	}
	
	#if UNITY_EDITOR
	/* [ContextMenu("Extract All Data to JSON")]
	void ExtractAllJson()
	{
		foreach(var book in books)
			book.ToJson();
	} */
	
	/* [ContextMenu("Arrange Books")]
	void ArrangeBooks()
	{
		var version = books[0].version;
			version.SetBooks(GetArrangedBooks());
		
		EditorUtility.SetDirty(version);
	} */
	#endif
	
	/* Book[] GetArrangedBooks()
	{
		int count = genInfo.bookChapterVerseInfos.Length;
		var arrangedBooks = new Book[count];
		
		for(int i = 0; i < count; i++)
		{
			var target = Array.Find(books, b => b.name == genInfo.bookChapterVerseInfos[i].name);	
			arrangedBooks[i] = target;
		}
		
		return arrangedBooks;
	} */
	
	#region UI
	
	// private void UpdateGraphUI(float duration, Book book)
	private void UpdateGraphUI(float duration, BookChapterVerseInfo bookInfo)
	{
		_graphTemplate.gameObject.SetActive(true);
		{
			var graph = Instantiate(_graphTemplate, _graphTemplate.transform.parent, false);
			
			if(duration > longestTime)
			{
				longestTime = duration;
				UpdateAllGraphs();
			}
			
			float percent = Mathf.Clamp01(duration / (longestTime * graphMultiplierNormalized));
			
			var img = graph.GetComponentInChildren<Image>();
				img.fillAmount = percent;
				img.color = Color.HSVToRGB(Mathf.Lerp(0, 0.8f, percent), 1, 1);
				
			var txt = graph.GetComponentInChildren<Text>();
				// txt.text = $"{book.version.NameCode}: <b>{book.nickname.ToUpper()}</b> - {(duration).ToString("0.0")} s";
				txt.text = $"{version.NameCode}: <b>{bookInfo.name}</b> - {(duration).ToString("0.0")} s";
				
				
			graphInstances.Add(new GraphInstance(img, duration));
		}
		_graphTemplate.gameObject.SetActive(false);	
	}
	
	public static void UpdateAllGraphs()
	{
		if(!Application.isPlaying) return;
		
		foreach(var instance in graphInstances)
		{
			float newPercent = Mathf.Clamp01(instance.sec / (longestTime * graphMultiplierNormalized));
			
			var img = instance.img;
				img.fillAmount = newPercent;
				img.color = Color.HSVToRGB(Mathf.Lerp(0, 0.8f, newPercent), 1, 1);
		}
	}
	
	public static void SortGraphInstances(bool sort)
	{
		if(sort)
		{
			var sorted = graphInstances.OrderBy(instance => instance.sec).ToList();
			
			for(int i = 0; i < sorted.Count; i++)
				sorted[i].img.transform.SetSiblingIndex(i);
		}
		else
		{
			for(int i = 0; i < graphInstances.Count; i++)
				graphInstances[i].img.transform.SetSiblingIndex(i);
		}
	}
	
	#endregion
}