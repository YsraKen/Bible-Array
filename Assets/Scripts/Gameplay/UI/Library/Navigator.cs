using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Navigator : MonoBehaviour
{
	public GeneralInformation genInfo;
	public GameObject _bookButtonTemplate, _chapterButtonTemplate;
	public Text _bookTxt;
	
	[Space]
	public ScrollRect _chapterSelectScroll;
	public float chaptersUpdateTotalDuration = 0.75f;
	public Toggle createNewTabToggle;
	
	[EnumData(typeof(ScreenOrientation))]
	public RectTransform[] _panelOrientationRefs;
	public RectTransform _panel;
	
	public bool createNewTab { get; set; }
	
	public static int SelectedBookIndex;
	Coroutine _updateChaptersRoutine;
	
	bool _started;
	
	void Start()
	{
		var bookBtnT = _bookButtonTemplate.transform;
		var chapBtnT = _chapterButtonTemplate.transform;
		
		var bookBtnPar = bookBtnT.parent;
		var chapBtnPar = chapBtnT.parent;
		
		for(int i = 0; i < genInfo.bookChapterVerseInfos.Length; i++)
			Instantiate(_bookButtonTemplate, bookBtnPar, false);
		
		for(int i = 0; i < genInfo.maxChapterCount; i++)
		{
			var btn = Instantiate(_chapterButtonTemplate, chapBtnPar, false);
				btn.GetComponentInChildren<Text>().text = $"{i + 1}";
		}
		
		bookBtnT.SetAsLastSibling();
		chapBtnT.SetAsLastSibling();
		
		_bookButtonTemplate.SetActive(false);
		_chapterButtonTemplate.SetActive(false);
		
		#region Initial Highlighting
		
		_bookTxt.text = genInfo.bookChapterVerseInfos[SelectedBookIndex].name;
		
		// for(int i = 0; i < bookBtnPar.childCount; i++)
		// {
			// var child = bookBtnPar.GetChild(i);
				// child.GetChild(0).gameObject.SetActive(i == SelectedBookIndex);
		// }
		
		#endregion
		
		_started = true;
		
		OnEnable();
	}
	
	public void OnEnable()
	{
		if(!_started) return;
		
		// var activeBibleVersion = GameManager.Instance.GetActiveBible().version;
		var activeBible = GameManager.Instance.GetActiveBible();
		
		for(int i = 0; i < genInfo.bookChapterVerseInfos.Length; i++)
		{
			int bookIndex = activeBible.version.Books[i];
			
			// string nickname = genInfo.bookChapterVerseInfos[i].name;
			// string fullname = activeBible.BookDatas[i].name;
			
			string name = bookIndex < 0? genInfo.bookChapterVerseInfos[i].name: activeBible.BookDatas[i].name;
			
			// Debug.Log($"{i} | {bookIndex} | {name} | {genInfo.bookChapterVerseInfos[bookIndex].name}", this);
			
			// var book = activeBibleVersion.Books.GetElement(i);
			// string name = book? book.Name: genInfo.bookChapterVerseInfos[i].name;
			
			var btn = _bookButtonTemplate.transform.parent.GetChild(i);
				btn.GetComponentInChildren<Text>().text = name;
		}
		
		
		OnBookSelect(_bookButtonTemplate.transform.parent.GetChild(SelectedBookIndex));
		UpdateChapters();
		
		AdjustScreenHeight(GameManager.Instance.ScreenOrientation);
	}
	
	void OnValidate()
	{
		if(Application.isPlaying)
			return;
		
		var mgr = FindObjectOfType<GameManager>();
		AdjustScreenHeight(mgr.ScreenOrientation);
	}
	
	public void OnBookSelect(Transform t)
	{
		SelectedBookIndex = t.GetSiblingIndex();
		
		#region Highlighting
		t.GetChild(0).gameObject.SetActive(true);
		
		for(int i = 0; i < t.parent.childCount; i++)
		{
			if(i == SelectedBookIndex)
				continue;
			
			var child = t.parent.GetChild(i);
				child.GetChild(0).gameObject.SetActive(false);
		}
		#endregion
		
		_bookTxt.text = genInfo.bookChapterVerseInfos[SelectedBookIndex].name;
		
		UpdateChapters();
		
		_chapterSelectScroll.verticalNormalizedPosition = 1f;
	}
	
	public void OnChapterSelect(Transform t)
	{
		int index = t.GetSiblingIndex();
		var mgr = GameManager.Instance;
		
		if(createNewTab)
		{
			mgr.AddTab(SelectedBookIndex, index);
			createNewTabToggle.isOn = false;
		}
		
		else
			mgr.NavigateTo(SelectedBookIndex, index);
		
		gameObject.SetActive(false);
	}
	
	void UpdateChapters()
	{
		if(_updateChaptersRoutine != null)
			StopCoroutine(_updateChaptersRoutine);
		
		_updateChaptersRoutine = StartCoroutine(r());
		
		IEnumerator r()
		{
			int chapters = genInfo.bookChapterVerseInfos[SelectedBookIndex].chaptersAndVerses.Length;
			var parent = _chapterButtonTemplate.transform.parent;
			
			for(int i = 0; i < genInfo.maxChapterCount; i++)
			{
				var child = parent.GetChild(i).gameObject;
					child.SetActive(false);
			}
			
			var step = new WaitForSeconds(chaptersUpdateTotalDuration / genInfo.maxChapterCount);
			
			for(int i = 0; i < genInfo.maxChapterCount; i++)
			{
				yield return step;
				
				var child = parent.GetChild(i).gameObject;
					child.SetActive(i < chapters);
			}
		}
	}
	
	public void AdjustScreenHeight(ScreenOrientation orientation)
	{
		_panel.sizeDelta = _panelOrientationRefs[(int) orientation].sizeDelta;
	}
}