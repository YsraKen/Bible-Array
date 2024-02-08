using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Navigator : MonoBehaviour
{
	public GeneralInformation genInfo;
	public GameObject _bookButtonTemplate, _chapterButtonTemplate;
	
	[Space]
	public ScrollRect _chapterSelectScroll;
	public float chaptersUpdateTotalDuration = 0.75f;
	
	[EnumData(typeof(ScreenOrientation)), Range(0,1)]
	public float[] screenHeightMultipliers;
	public RectTransform _screenHeightRef;
	
	int _selectedBookIndex;
	Coroutine _updateChaptersRoutine;
	
	bool _started;
	
	void Start()
	{
		var backBtnT = _bookButtonTemplate.transform;
		var chapBtnT = _chapterButtonTemplate.transform;
		
		var backBtnPar = backBtnT.parent;
		var chapBtnPar = chapBtnT.parent;
		
		for(int i = 0; i < genInfo.bookChapterVerseInfos.Length; i++)
		{
			var btn = Instantiate(_bookButtonTemplate, backBtnPar, false);
				btn.GetComponentInChildren<Text>().text = genInfo.bookChapterVerseInfos[i].name;
		}
		
		for(int i = 0; i < genInfo.maxChapterCount; i++)
		{
			var btn = Instantiate(_chapterButtonTemplate, chapBtnPar, false);
				btn.GetComponentInChildren<Text>().text = $"{i + 1}";
		}
		
		backBtnT.SetAsLastSibling();
		chapBtnT.SetAsLastSibling();
		
		_bookButtonTemplate.SetActive(false);
		_chapterButtonTemplate.SetActive(false);
		
		_started = true;
		
		OnEnable();
	}
	
	public void OnEnable()
	{
		if(!_started) return;
		
		UpdateChapters();
		AdjustScreenHeight(GameManager.Instance.ScreenOrientation);
	}
	
	public void OnBookSelect(Transform t)
	{
		_selectedBookIndex = t.GetSiblingIndex();
		
		UpdateChapters();
		
		_chapterSelectScroll.verticalNormalizedPosition = 1f;
	}
	
	public void OnChapterSelect(Transform t)
	{
		int index = t.GetSiblingIndex();
		
		GameManager.Instance.NavigateTo(_selectedBookIndex, index);
		gameObject.SetActive(false);
	}
	
	void UpdateChapters()
	{
		if(_updateChaptersRoutine != null)
			StopCoroutine(_updateChaptersRoutine);
		
		_updateChaptersRoutine = StartCoroutine(r());
		
		IEnumerator r()
		{
			int chapters = genInfo.bookChapterVerseInfos[_selectedBookIndex].chaptersAndVerses.Length;
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
		_screenHeightRef.sizeDelta = new Vector2
		(
			_screenHeightRef.sizeDelta.x,
			GameManager.Instance.ScreenSize.y * screenHeightMultipliers[(int) orientation]
		);
	}
}