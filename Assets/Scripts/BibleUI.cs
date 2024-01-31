using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BibleUI : MonoBehaviour
{
	[SerializeField] private Book _book;
	[SerializeField] private VerseUI _verseUiTemplate;
	[SerializeField] private string _verseCommentLink;
	[SerializeField] private Text _headerTxt;
	[SerializeField] private VerticalLayoutGroup _vlg;
	[SerializeField] private TMP_Text _popupTitleTmp;
	[SerializeField] private TMP_Text _popupTmp;
	
	public int chapterIndex;
	private VerseUI[] _verseUis;
	
	private static TMP_Text _popupTitle;
	private static TMP_Text _popup;
	
	void Awake()
	{
		_popupTitle = _popupTitleTmp;
		_popup = _popupTmp;
	}
	
	void Start()
	{
		_headerTxt.text = $"{_book.name} {chapterIndex + 1}/{_book.chapters.Length}";
		
		var chapter = _book.chapters[chapterIndex];
		int verseCount = chapter.verses.Length;
		
		_verseUis = new VerseUI[verseCount];
		var parent = _verseUiTemplate.transform.parent;
		
		_verseUiTemplate.gameObject.SetActive(true);
		
		for(int i = 0; i < verseCount; i++)
		{
			var verseUI = Instantiate(_verseUiTemplate, parent, false);
				verseUI.Setup(chapter.verses[i], i, _verseCommentLink);
			
			_verseUis[i] = verseUI;
		}
		
		_verseUiTemplate.gameObject.SetActive(false);
		_verseUiTemplate.transform.SetAsLastSibling();
	}
	
	public void NavigateChapter(int dir)
	{
		chapterIndex += dir;
		
		if(_book.chapters.IsInsideRange(chapterIndex))
		{
			for(int i = 0; i < _verseUis.Length; i++)
				Destroy(_verseUis[i].gameObject);
			
			Start();
		}
		
		StartCoroutine(r());
		IEnumerator r()
		{
			_vlg.childControlWidth = !_vlg.childControlWidth;
			yield return null;
			_vlg.childControlWidth = !_vlg.childControlWidth;
		}
	}
	
	public static void ShowPopup(string title, string body, Vector3 position)
	{
		_popupTitle.text = title;
		_popup.text = body;
		
		var transform = _popup.transform.parent;
			transform.position = position;
			transform.gameObject.SetActive(true);
	}
	
	Vector3 _repositionOffset;
	
	public void Reposition_Start(Transform transform) => _repositionOffset = transform.position - Input.mousePosition;
	public void Reposition(Transform transform) => transform.position = Input.mousePosition + _repositionOffset;
}