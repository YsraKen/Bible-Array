using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

public class BibleUI2 : MonoBehaviour
{
	public Version version;
	
	[SerializeField] private TMP_Text _chapterTmp;
	[SerializeField] private GameObject _bookInfoButton;
	
	[SerializeField] private VerseUI2 _verseTemplate;
	
	[HideInInspector]
	public List<VerseUI2> _verseInstances = new List<VerseUI2>();
	
	[SerializeField] private Transform _header;
	[SerializeField] private Transform _headerClampTarget;
	
	[field: SerializeField]
	public GameObject Glow { get; private set; }
	
	[SerializeField] private TMP_Text _versionTmp;
	// [SerializeField] private TMP_Dropdown _options;
	
	[Space]
	[SerializeField] private RectTransform _rectTransform;
	[SerializeField] private RectTransform _body;
	
	[Space]
	[SerializeField] private float _headSizeOffset = 40f;
	[SerializeField] private float _footSizeOffset = 5f;
	
	[SerializeField] private float _verseSpacing = 2.5f;
	[SerializeField] private float _verseBodyPaddingSize = 7.5f;
	
	public RectTransform Body => _body;
	
	static GameManager _mgr;
	
	IEnumerator Start()
	{
		_mgr = GameManager.Instance;
		_mgr.BibleInstances.Add(this);
		_mgr.MainContentLoadingOverlay.SetActive(true);
		
		yield return new WaitUntil(()=> _mgr.Started);
		
		var sizeDelta = _rectTransform.sizeDelta;
			sizeDelta.x = _mgr.ScreenSize.x;
		
		if(sizeDelta.x > _mgr.DefaultMinWidth)
			sizeDelta.x = _mgr.DefaultMinWidth;
		
		sizeDelta.x -= (sizeDelta.x * 0.0375f);
		_rectTransform.sizeDelta = sizeDelta;
		
		yield return null;
		
		if(!_mgr.IsLoadingContent)
			_mgr.MainContentLoadingOverlay.SetActive(false);
	}
	
	void LateUpdate()
	{
		// Clamping header
		float headClampTarget = _headerClampTarget.position.y;
		float screenTopPos = _mgr.HeadersClampRef.position.y;
		float hudHeaderOffset = screenTopPos - 65f;
		
		float positionY = _header.position.y;
		positionY = headClampTarget > screenTopPos? screenTopPos: headClampTarget;
		
		_header.position = new Vector3(_header.position.x, positionY, _header.position.z);
	}
	
	void OnDestroy()
	{
		_mgr.BibleInstances.Remove(this);
	}
	
	[ContextMenu("Update Contents")]
	public void UpdateContents()
	{
		foreach(var verseInstance in _verseInstances)
			Destroy(verseInstance.gameObject);
		
		_verseInstances.Clear();
		
		int bookIndex = _mgr.CurrentBookIndex;
		int chapterIndex = _mgr.CurrentChapterIndex;
		
		var book = version.Books[bookIndex];
		
		_versionTmp.text = version.Name;
		_chapterTmp.text = "";
		
		if(!book) return;
		
		var chapter = book.chapters[chapterIndex];
		
		bool isFirstChapter = chapterIndex == 0;
		string bookName = isFirstChapter? book.fancyName: book.Name;
		
		if(string.IsNullOrEmpty(bookName))
			bookName = book.Name;
		
		_chapterTmp.text += $"\n<size={_chapterTmp.fontSize * 2f}>{chapterIndex + 1}</size>\n{bookName}\n\n";
		
		bool hasBookDescription = isFirstChapter && !string.IsNullOrEmpty(book.description);
		_bookInfoButton.SetActive(hasBookDescription);
		
		_verseTemplate.gameObject.SetActive(true);
		{
			int index = 0;
			Verse previousVerse = null;
			
			foreach(var verse in chapter.verses)
			{
				bool isDuplicated = previousVerse != null? verse.number == previousVerse.number: false;
				
				var instance = _verseTemplate.Create(index ++, verse, isDuplicated);
					instance.bible = this;
					instance.LoadData();
				
				_verseInstances.Add(instance);
				previousVerse = verse;
			}
		}		
		_verseTemplate.gameObject.SetActive(false);
		_verseTemplate.transform.SetAsLastSibling();
	}
	
	[ContextMenu("Update Vertical Size")]
	public void UpdateVerticalSize()
	{
		UpdateVerseSizes();
		AdjustBodySize();
	}
	
	[ContextMenu("Update Verse Sizes")]
	public void UpdateVerseSizes()
	{
		for(int i = 0; i < _body.childCount; i++)
		{
			var verse = _body.GetChild(i) as RectTransform;
			var verseBody = verse.GetChild(0) as RectTransform;
			
			float totalPaddingSize = _verseBodyPaddingSize * 2;
			verse.sizeDelta = new Vector2(verse.sizeDelta.x, verseBody.sizeDelta.y + totalPaddingSize);
		}
	}
	
	[ContextMenu("Adjust Body Size")]
	public void AdjustBodySize()
	{
		// Body
		float versesTotalSize = 0;
		
		for(int i = 0; i < _body.childCount; i++)
		{
			var verse = _body.GetChild(i) as RectTransform;
				versesTotalSize += verse.sizeDelta.y + (_verseSpacing * 0.5f);
		}
		_body.sizeDelta = new Vector2(_body.sizeDelta.x, versesTotalSize);
		
		// Self
		float verticalOffset = _headSizeOffset + _footSizeOffset;
		
		_rectTransform.sizeDelta = new Vector2
		(
			_rectTransform.sizeDelta.x,
			_body.sizeDelta.y + verticalOffset
		);
	}
	
	public void OpenVersionSelector() => _mgr.OpenVersionSelector(this);
	
	public void SetVersion(Version version)
	{
		if(version == this.version)
			return;
		
		Debug.Log(version, version);
		this.version = version;
		
		UpdateContents();
	}
	
	public void OnBookInfoClick()
	{
		var bookInfo = version.Books[_mgr.CurrentBookIndex];
		_mgr.PreviewBookInfo(bookInfo);
	}
	
	public void OnDelete() => Destroy(gameObject);
}