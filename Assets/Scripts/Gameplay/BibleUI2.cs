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
	[SerializeField] private VerseUI2 _verseTemplate;
	
	[HideInInspector]
	public List<VerseUI2> _verseInstances = new List<VerseUI2>();
	
	[SerializeField] private Transform _header;
	[SerializeField] private Transform _headerClampTarget;
	
	[SerializeField] private TMP_Text _versionTmp;
	// [SerializeField] private TMP_Dropdown _options;
	
	[Space]
	[SerializeField] private RectTransform _rectTransform;
	[SerializeField] private RectTransform _body;
	
	[field: SerializeField, FormerlySerializedAs("_cover")]
	public GameObject Cover { get; private set; }
	
	[Space]
	[SerializeField] private float _headSizeOffset = 40f;
	[SerializeField] private float _footSizeOffset = 5f;
	
	[SerializeField] private float _verseSpacing = 2.5f;
	[SerializeField] private float _verseBodyPaddingSize = 7.5f;
	
	public RectTransform Body => _body;
	
	static GameManager _mgr;
	
	IEnumerator Start()
	{
		Cover.SetActive(true);
		
		_mgr = GameManager.Instance;
		yield return new WaitUntil(()=> _mgr.Started);
		
		var sizeDelta = _rectTransform.sizeDelta;
			sizeDelta.x = _mgr.ScreenSize.x;
		
		if(sizeDelta.x > _mgr.DefaultMinWidth)
			sizeDelta.x = _mgr.DefaultMinWidth;
		
		sizeDelta.x -= (sizeDelta.x * 0.0375f);
		_rectTransform.sizeDelta = sizeDelta;
		
		yield return null;
		Cover.SetActive(false);
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
	
	[ContextMenu("Update Contents")]
	public void UpdateContents()
	{
		foreach(var verseInstance in _verseInstances)
			Destroy(verseInstance.gameObject);
		
		_verseInstances.Clear();
		
		int bookIndex = _mgr.CurrentBookIndex;
		int chapterIndex = _mgr.CurrentChapterIndex;
		
		var book = version.Books[bookIndex];
		// _versionTmp.text = $"<b>[{version.NameCode}]</b>";
		_versionTmp.text = version.Name;
		
		if(!book) return;
		
		var chapter = book.chapters[chapterIndex];
		
		// _versionTmp.text += $" {book.Name} {chapterIndex + 1}";
		
		string bookName = book.fancyName;
		
		if(string.IsNullOrEmpty(bookName))
			bookName = book.Name;
		
		_chapterTmp.text = $"<size={_chapterTmp.fontSize * 2f}>{chapterIndex + 1}</size>\n{bookName}";
		
		_verseTemplate.gameObject.SetActive(true);
		{
			int index = 0;
			
			Verse previousVerse = null;
			_verseTemplate.bible = this;
			
			foreach(var verse in chapter.verses)
			{
				bool isDuplicated = previousVerse != null? verse.number == previousVerse.number: false;
				
				var instance = _verseTemplate.Create(index ++, verse, isDuplicated);
				
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
	
	/* public void OnOptionsOpen()
	{
		_options.ClearOptions();
		
		var options = new List<TMP_Dropdown.OptionData>();
		var _mgr = _mgr;
		
		foreach(var option in _mgr.BibleOptionsDefaultList)
			options.Add(option);
		
		int recents = _mgr.recents.Count;
		
		if(recents > 0)
		{
			var recent = _mgr.recents[recents - 1];
			options.Add(new TMP_Dropdown.OptionData(recent.NameCode, _mgr.RecentIcon));
		}
		
		foreach(var favorite in _mgr.favorites)
			options.Add(new TMP_Dropdown.OptionData(favorite.NameCode, _mgr.FavoritesIcon));
		
		_options.AddOptions(options);
	}
	
	public void OnOptionsSelect()
	{
		StartCoroutine(r());
		IEnumerator r()
		{
			int index = _options.value;
			_options.Hide();
			
			yield return new WaitForSeconds(0.15f);
			
			switch(index)
			{
				case 0:
					var newInstance = Instantiate(gameObject, transform.parent, false);
					int myIndex = transform.GetSiblingIndex();
					
					newInstance.transform.SetSiblingIndex(myIndex + 1);
					
					break;
				
				case 1:
					Destroy(gameObject);
					break;
			}
		}
	} */
	
	public void OnDelete() => Destroy(gameObject);
}