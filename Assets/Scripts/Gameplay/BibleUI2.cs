using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BibleUI2 : MonoBehaviour
{
	public Version version;
	
	[SerializeField] private VerseUI2 _verseTemplate;
	
	public List<VerseUI2> _verseInstances = new List<VerseUI2>();
	
	[SerializeField] private TMP_Text _versionTmp;
	[SerializeField] private TMP_Dropdown _options;
	
	[Space]
	[SerializeField] private RectTransform _rectTransform;
	[SerializeField] private RectTransform _body;
	
	[SerializeField] private float _headSizeOffset = 40f;
	[SerializeField] private float _footSizeOffset = 5f;
	
	[SerializeField] private float _verseSpacing = 2.5f;
	[SerializeField] private float _verseBodyPaddingSize = 7.5f;
	
	public RectTransform Body => _body;
	
	[ContextMenu("Update Contents")]
	public void UpdateContents()
	{
		int bookIndex = GameManager.Instance.CurrentBookIndex;
		int chapterIndex = GameManager.Instance.CurrentChapterIndex;
		
		var book = version.Books[bookIndex];
		var chapter = book.chapters[chapterIndex];
		
		_versionTmp.text = $"<b>[{version.NameCode}]</b> {book.Name} {chapterIndex + 1}";
		
		foreach(var verseInstance in _verseInstances)
			Destroy(verseInstance.gameObject);
		
		_verseInstances.Clear();
		
		_verseTemplate.gameObject.SetActive(true);
		{
			int index = 0;
			
			foreach(var verse in chapter.verses)
				_verseInstances.Add(_verseTemplate.Create(verse, index ++));
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
	
	public void OpenVersionSelector() => GameManager.Instance.OpenVersionSelector(this);
	
	public void SetVersion(Version version)
	{
		if(version == this.version)
			return;
		
		Debug.Log(version, version);
		this.version = version;
		
		UpdateContents();
	}
	
	public void OnOptionsOpen()
	{
		_options.ClearOptions();
		
		var options = new List<TMP_Dropdown.OptionData>();
		var mgr = GameManager.Instance;
		
		foreach(var option in mgr.BibleOptionsDefaultList)
			options.Add(option);
		
		int recents = mgr.recents.Count;
		
		if(recents > 0)
		{
			var recent = mgr.recents[recents - 1];
			options.Add(new TMP_Dropdown.OptionData(recent.NameCode, mgr.RecentIcon));
		}
		
		foreach(var favorite in mgr.favorites)
			options.Add(new TMP_Dropdown.OptionData(favorite.NameCode, mgr.FavoritesIcon));
		
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
	}
}