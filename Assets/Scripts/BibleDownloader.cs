using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BibleDownloader : MonoBehaviour
{
	public Book[] books;
	
	public string urlPre;
	public string urlPost;
	public string htmlStartRead;
	
	[Space]
	public Book currentBook;
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
	private static Image _graphTemplate;
	
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
			_graphTemplate = GameObject.FindWithTag("Download Graph").GetComponent<Image>();
	}
	
	IEnumerator Start()
	{
		IsDone = false;
		StartTime = Time.time;
		
		for(int i = 0; i < books.Length; i++)
		{
			allProgress = (float) i / (float) (books.Length - 1);
			
			currentBook = books[i];
			yield return Download(currentBook);
		}
		
		var version = books[0].version;
			version.SetBooks(GetArrangedBooks());
		
		yield return null;
		
		float duration = Time.time - StartTime;
		Debug.Log($"<b>EXIT! duration: <color=green>'{duration} seconds'</color></b>");
		
		IsDone = true;
	}
	
	IEnumerator Download(Book book)
	{
		int numberOfChapters = book.chapters.Length;
		BookStartTime = Time.time;
		
		for(int i = 0; i < numberOfChapters; i++)
		{
			currentChapter = i + 1;
			
			string url = $"{urlPre}{book.name}.{i + 1}{urlPost}";
			string html = "";
			
			if(_urlTxt)
				_urlTxt.text = url;
			
			yield return GetWebData(url, downloadData => html = downloadData);
			
			int htmlStartIndex = html.IndexOf(htmlStartRead);
			html = html.Substring(htmlStartIndex);
			
			yield return null;
			
			extractor.text = html;
			extractor.Extract();
			
			yield return null;
			
			var chapter = new Chapter(){ verses = extractor.verses.ToArray() };
			book.chapters[i] = chapter;
			
			bookProgress = (float) i / (float) (numberOfChapters - 1);
		}
		
		book.SetName(extractor.bookName);
		
		float duration = Time.time - BookStartTime;
		Debug.Log($"Done: <b>{book.version.NameCode}: <color=cyan>{book.name}</color></b>. duration: <color=yellow>'{duration.ToString("0.00")} seconds'</color>");
		
		#if UNITY_EDITOR
		EditorUtility.SetDirty(book);
		#endif
		
		if(_graphTemplate)
			UpdateGraphUI(duration, book);
		
		yield return null;
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
	
	#if UNITY_EDITOR
	[ContextMenu("Extract All Data to JSON")]
	void ExtractAllJson()
	{
		foreach(var book in books)
			book.ToJson();
	}
	
	[ContextMenu("Arrange Books")]
	void ArrangeBooks()
	{
		var version = books[0].version;
			version.SetBooks(GetArrangedBooks());
		
		EditorUtility.SetDirty(version);
	}
	#endif
	
	Book[] GetArrangedBooks()
	{
		int count = genInfo.bookChapterVerseInfos.Length;
		var arrangedBooks = new Book[count];
		
		for(int i = 0; i < count; i++)
		{
			var target = Array.Find(books, b => b.name == genInfo.bookChapterVerseInfos[i].name);	
			arrangedBooks[i] = target;
		}
		
		return arrangedBooks;
	}
	
	#region UI
	
	private void UpdateGraphUI(float duration, Book book)
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
			
			graph.fillAmount = percent;
			graph.color = Color.HSVToRGB(Mathf.Lerp(0, 0.8f, percent), 1, 1);
			graph.GetComponentInChildren<Text>().text = $"<b>{book.version.NameCode}:</b> <i>{book.nickname.ToUpper()}</i> - {(duration).ToString("0.0")} s";
			
			graphInstances.Add(new GraphInstance(graph, duration));
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