using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class GameManager : MonoBehaviour
{
	#region Variables and Properties
	
	[SerializeField] private Language[] _languages;
	[SerializeField] private Version[] _versions;
	
	public Collection[] Collections { get; private set; }
	
	[Space]
	[SerializeField] private BibleUI2 _biblePrefab;
	[SerializeField] private Version[] _onStartBibles;
	
	[field: SerializeField] public GeneralInformation GeneralInfo { get; private set; }
	[field: SerializeField] public Color JesusWordColor { get; private set; } = Color.red;
	[field: SerializeField] public string VerseCommentLink { get; private set; }
	
	[field: SerializeField]
	public List<BibleUI2> BibleInstances { get; private set; } = new List<BibleUI2>();
	
	[field: SerializeField] public int CurrentBookIndex { get; private set; }
	[field: SerializeField] public int CurrentChapterIndex { get; private set; }
	
	[Space]
	[SerializeField] private GameObject _tabTemplate;
	[SerializeField] private ScrollRect _tabsScroll;
	
	private List<Tab> _tabs = new List<Tab>();
	private int _currentTabIndex;
	private Coroutine _tabRoutine;
	
	[SerializeField] private ScrollRect _mainScroll;
	[SerializeField] private float _hudReactionThresholdToMainScrollVelocity = 100f;
	
	[SerializeField] private BibleLayout _layout;
	
	[field: SerializeField]
	public GameObject MainContentLoadingOverlay { get; private set; }
	public bool IsLoadingContent { get; private set; }
	
	[field: SerializeField]
	public Sprite RecentIcon { get; private set; }
	
	[field: SerializeField]
	public Sprite FavoritesIcon { get; private set; }
	
	[Space]
	[SerializeField] private GameObject _versionSelectPanel;
	[SerializeField] private TMP_Dropdown _languageSelector;
	
	[SerializeField] private VersionSelectPanel _versionSelect;
	private BibleUI2 _versionSelectTargetBible;
	
	[SerializeField] private Animation[] _hudAnimations;
	private Vector2 _lastScrollPosition;
	
	private bool _appearHud_OneFrameGate;
	public bool IsHudShown { get; private set; } = true;
	
	[Space]
	[SerializeField] private BookInfoUI _bookInfoPanel;
	[SerializeField] private TMP_Text _popupTitle;
	[SerializeField] private TMP_Text _popup;
	
	[Space]
	[SerializeField] private GameObject _verseOptions;
	
	Vector3 _repositionOffset;
	
	public List<VerseUI2> SelectedVerses { get; private set; } = new List<VerseUI2>(); // or use Dictionary
	public Action onVerseSelectUpdate;
	
	[SerializeField] private RectTransform _screenSizeRef;
	
	[field: SerializeField]
	public Transform HeadersClampRef { get; private set; }
	
	[SerializeField] private GameObject _quitPanel;
	
	public Vector2 ScreenSize => _screenSizeRef.rect.size;
	public float DefaultMinWidth { get; private set; }
	
	public UnityEvent<ScreenOrientation> onScreenOrientationChanged;
	public ScreenOrientation ScreenOrientation { get; private set; }
	ScreenOrientation _previousScreenOrientation;
	
	public static GameManager Instance { get; private set; }
	public bool Started { get; private set; }
	
	#endregion
	
	void Awake()
	{
		Started = false;
		Instance = this;
		
		if(PlayerPrefs.HasKey("CurrentBookIndex"))
			CurrentBookIndex = PlayerPrefs.GetInt("CurrentBookIndex");
		
		if(PlayerPrefs.HasKey("CurrentChapterIndex"))
			CurrentChapterIndex = PlayerPrefs.GetInt("CurrentChapterIndex");
	}
	
	IEnumerator Start()
	{
		int langCount = _languages.Length;
		Collections = new Collection[langCount];
		
		_languageSelector.ClearOptions();
		var languageSelectorOptions = new List<string>();
		
		for(int i = 0; i < langCount; i++)
		{
			var language = _languages[i];
			var versions = new List<Version>();
			
			Collections[i] = new Collection(){ language = language };
			
			foreach(var version in _versions)
			{
				if(version.Language == language)
					versions.Add(version);
			}
			
			Collections[i].versions = versions;
			languageSelectorOptions.Add(language.Name);
		}
		
		_languageSelector.AddOptions(languageSelectorOptions);
		yield return null;
		
		if(PlayerPrefs.HasKey("OnStartBibles"))
		{
			Debug.Log("OnStartBibles");
			
			int onStartBibleCount = PlayerPrefs.GetInt("OnStartBibles_Count");
			Debug.Log("OnStartBibles_Count: " + onStartBibleCount);
			
			if(onStartBibleCount == 0)
				Array.ForEach(_onStartBibles, osb => CreateBibleInstance(osb));
			
			else
			{
				for(int i = 0; i < onStartBibleCount; i++)
				{
					int versionIndex = PlayerPrefs.GetInt($"OnStartBibles_{i}");
					Debug.Log($"OnStartBibles_{i}: {versionIndex}");
					
					var version = _versions[versionIndex];
					CreateBibleInstance(version);
					
					yield return null;
				}
			}
		}
		else
			Array.ForEach(_onStartBibles, osb => CreateBibleInstance(osb));
		
		yield return null;
		
		DefaultMinWidth = Mathf.Min(ScreenSize.x, ScreenSize.y);
		
		AddTab(CurrentBookIndex, CurrentChapterIndex);
		UpdateBibleContents();
		
		yield return new WaitWhile(()=> IsLoadingContent);
		
		if(PlayerPrefs.HasKey("mainScrollPosX") && PlayerPrefs.HasKey("mainScrollPosY"))
		{
			var scrollPosition = new Vector2
			(
				PlayerPrefs.GetFloat("mainScrollPosX"),
				PlayerPrefs.GetFloat("mainScrollPosY")
			);
			
			yield return _mainScroll.SetPositionRoutine(scrollPosition, 0.65f);
		}
		
		Started = true;
	}
	
	void Update()
	{
		var screenSize = ScreenSize;
		
		ScreenOrientation = screenSize.x < screenSize.y?
			ScreenOrientation.Vertical:
			ScreenOrientation.Horizontal;
		
		if(ScreenOrientation != _previousScreenOrientation)
			onScreenOrientationChanged?.Invoke(ScreenOrientation);
		
		_previousScreenOrientation = ScreenOrientation;
		
		if(Input.GetButtonDown("Cancel"))
			_quitPanel.SetActive(true);
	}
	
	BibleUI2 CreateBibleInstance(Version version)
	{
		var instance = Instantiate(_biblePrefab, _layout.transform, false);
			instance.version = version;
		
		// BibleInstances.Add(instance);
		return instance;
	}
	
	void UpdateBibleContents()
	{
		StartCoroutine(r());
		IEnumerator r()
		{
			IsLoadingContent = true;
			MainContentLoadingOverlay.SetActive(true);
			
			var commentPopup = _popupTitle.transform.parent.gameObject;
				commentPopup.SetActive(false);
			
			yield return null;
			
			foreach(var instance in BibleInstances)
			{
				instance.UpdateContents();
				instance.UpdateVerticalSize();
			}
			
			yield return null;
			_layout.Repaint();
			
			yield return null;
			
			MainContentLoadingOverlay.SetActive(false);
			IsLoadingContent = false;
		}
	}
	
	#region Version Select
	
	public void OpenVersionSelector(BibleUI2 target)
	{
		_versionSelectTargetBible = target;
		_versionSelect.StartSelection(OnVersionSelect);
	}
	
	public void AddVersion() => _versionSelect.StartSelection(OnVersionSelect);
	
	public void UpdateVersionSelectList() => _versionSelect.UpdateList();
	
	public void OnVersionSelect(Version version)
	{
		_versionSelectPanel.SetActive(false);
		bool hasCreatedNewInstance = false;
		
		#region Gates
		
		if(!_versionSelectTargetBible)
		{
			if(isVersionExists_UpdateScrollPosition())
				return;
			
			_versionSelectTargetBible = CreateBibleInstance(version);
			hasCreatedNewInstance = true;
		}
		
		else if(_versionSelectTargetBible.version == version)
		{
			NullifyVersionSelectTargetBible();
			return;
		}
		
		if(isVersionExists_UpdateScrollPosition())
		{
			NullifyVersionSelectTargetBible();
			return;
		}
		
		#endregion
		
		StartCoroutine(r());
		
		#region Sub Methods
		
		bool isVersionExists_UpdateScrollPosition()
		{
			bool output = BibleInstances.TryFindIndex(instance => instance.version == version, out int index);
			
			if(output)
			{
				float scrollPosition = Mathf.InverseLerp(0, BibleInstances.Count - 1, index);
				StartCoroutine(updateScrollPosition(scrollPosition, BibleInstances[index].Glow));
			}
			
			return output;
		}
		
		IEnumerator r()
		{
			_versionSelectTargetBible.SetVersion(version);
			_versionSelect.AddRecents(version);
			
			yield return null;
			_versionSelectTargetBible.UpdateContents();
			
			yield return null;
			
			UpdateBibleContents();
			NullifyVersionSelectTargetBible();
			
			if(hasCreatedNewInstance)
			{
				var instance = BibleInstances[BibleInstances.Count - 1];
				yield return updateScrollPosition(1f, instance.Glow);
			}
			else
			{
				yield return new WaitWhile(()=> IsLoadingContent);
				
				var instance = BibleInstances.Find(instance => instance.version == version);
					instance.Glow.SetActive(true);
			}
		}
		
		IEnumerator updateScrollPosition(float normalizedPosition, GameObject glow)
		{
			var position = new Vector2
			(
				normalizedPosition,
				_mainScroll.normalizedPosition.y
			);
			
			yield return _mainScroll.SetPositionRoutine(position, 0.75f);
			
			glow.SetActive(true);
		}
		
		#endregion
	}
	
	public void NullifyVersionSelectTargetBible() => _versionSelectTargetBible = null;
	
	#endregion
	
	public void OnScroll(Vector2 value)
	{
		float velocity = Mathf.Abs(_mainScroll.velocity.y);
		
		if(velocity < _hudReactionThresholdToMainScrollVelocity)
			return;
		
		if(value.y > 0.95f || value.y < 0.05f)
		{
			if(!_appearHud_OneFrameGate)
			{
				Array.ForEach(_hudAnimations, animation => animation.Play("hudAppear"));
				_appearHud_OneFrameGate = true;
			}
			
			IsHudShown = true;
			return;
		}
	
		if(value.y > _lastScrollPosition.y)
		{
			if(!_appearHud_OneFrameGate)
			{
				Array.ForEach(_hudAnimations, animation => animation.Play("hudAppear"));
				_appearHud_OneFrameGate = true;
			}
			
			IsHudShown = true;
		}
		
		else
		{
			if(_appearHud_OneFrameGate)
			{
				Array.ForEach(_hudAnimations, animation => animation.Play("hudDisappear"));
				_appearHud_OneFrameGate = false;
			}
			
			IsHudShown = false;
		}
		
		_lastScrollPosition = value;
	}
	
	public void IterateChapter(int dir)
	{
		dir = Mathf.Clamp(dir, -1, 1);
		CurrentChapterIndex += dir;
		
		var version = _versions[0];
		
		if(CurrentChapterIndex < 0)
		{
			CurrentBookIndex --;
			
			if(CurrentBookIndex < 0)
				CurrentBookIndex = version.Books.Length - 1;
			
			CurrentChapterIndex = version.Books[CurrentBookIndex].chapters.Length - 1;
		}
		
		if(CurrentChapterIndex >= version.Books[CurrentBookIndex].chapters.Length)
		{
			CurrentBookIndex ++;
			
			if(CurrentBookIndex >= version.Books.Length)
				CurrentBookIndex = 0;
			
			CurrentChapterIndex = 0;
		}
		
		UpdateBibleContents();
		UpdateTabText();
		
		_mainScroll.verticalNormalizedPosition = 1;
	}
	
	[ContextMenu("Test")]
	public void Test()
	{
		int bookIndex = UnityEngine.Random.Range(0, 66);
		int chapterCount = GeneralInfo.bookChapterVerseInfos[bookIndex].chaptersAndVerses.Length;
		int chapterIndex = UnityEngine.Random.Range(0, chapterCount);
		
		NavigateTo(bookIndex, chapterIndex);
	}
	
	public void NavigateTo(int bookIndex, int chapterIndex)
	{
		CurrentBookIndex = bookIndex;
		CurrentChapterIndex = chapterIndex;
		
		UpdateBibleContents();
		_mainScroll.verticalNormalizedPosition = 1;
		
		// TABS
		UpdateTabText();
		
		_tabs.ForEach(tab => tab.SetHighlight(false));
		_tabs[_currentTabIndex].SetHighlight(true);
		
		_tabs[_currentTabIndex].bookIndex = bookIndex;
		_tabs[_currentTabIndex].chapterIndex = chapterIndex;
	}
	
	public void PreviewBookInfo(Book book)
	{
		string bookName = string.IsNullOrEmpty(book.fancyName)? book.Name: book.fancyName;
		string title = $"<b>[{book.version.NameCode}]</b> {bookName}";
		
		_bookInfoPanel.Show(book.description, title);
	}
	
	public void ShowVerseCommentPopup(Version version, int index, int commentIndex, Vector3 position)
	{
		var comment = version.Books[CurrentBookIndex][CurrentChapterIndex][index][commentIndex];
		
		_popupTitle.text = comment.number;
		_popup.text = comment.content;
		
		var transform = _popupTitle.transform.parent;
			transform.position = position;
			transform.gameObject.SetActive(true);
	}
	
	public void OnVerseSelect(VerseUI2 verseUI)
	{
		if(SelectedVerses.Contains(verseUI, out int index))
		{
			verseUI.OnSelectionHighlight(false);
			SelectedVerses.RemoveAt(index);
		}
		else
			SelectedVerses.Add(verseUI);
		
		SelectedVerses.ForEach(selected => selected.OnSelectionHighlight(true));
		_verseOptions.SetActive(SelectedVerses.Count > 0);
		
		onVerseSelectUpdate?.Invoke();
	}
	
	public void Reposition_Start(Transform transform) => _repositionOffset = transform.position - Input.mousePosition;
	public void Reposition(Transform transform) => transform.position = Input.mousePosition + _repositionOffset;
	
	public BibleUI2 GetActiveBible()
	{
		int index = Mathf.RoundToInt(Mathf.Lerp(0, BibleInstances.Count - 1, _mainScroll.horizontalNormalizedPosition));
		return BibleInstances[index];
	}
	
	#region Tabs
	
	public void AddTab(int bookIndex, int chapterIndex)
	{
		StartCoroutine(r());
		IEnumerator r()
		{
			var templateT = _tabTemplate.transform;
			
			_tabTemplate.SetActive(true);
			
			var tab = Instantiate(_tabTemplate, templateT.parent, false);
			var txt = tab.GetComponentInChildren<Text>();
			
			var info = new Tab()
			{
				gameObject = tab,
				txt = txt,
				bookIndex = bookIndex,
				chapterIndex = chapterIndex
			};
			
			_tabs.Add(info);
			_currentTabIndex = _tabs.Count - 1;
			
			_tabTemplate.SetActive(false);
			templateT.SetAsLastSibling();
			
			yield return null;
			
			_tabsScroll.horizontalNormalizedPosition = 1f;
			NavigateTo(bookIndex, chapterIndex);
			
			yield return new WaitWhile(()=> IsLoadingContent);
			
			var glow = tab.transform.GetChild(0).GetChild(0);
				glow.gameObject.SetActive(true);
		}
	}
	
	private void UpdateTabText()
	{
		var tab = _tabs[_currentTabIndex];
			tab.txt.text = $"{GeneralInfo.bookChapterVerseInfos[CurrentBookIndex].name} {CurrentChapterIndex + 1}";
	}
	
	public void OnTabSelect(Transform transform) => OnTabSelect(transform.GetSiblingIndex());
	
	public void OnTabSelect(int index)
	{
		_currentTabIndex = index % _tabs.Count;
		var tab = _tabs[_currentTabIndex];
		
		NavigateTo(tab.bookIndex, tab.chapterIndex);
		
		if(_tabRoutine != null)
			StopCoroutine(_tabRoutine);
		
		#region Set Scrolling
		
		_tabRoutine = StartCoroutine(r());
		IEnumerator r()
		{
			yield return new WaitWhile(() => IsLoadingContent);
			yield return null;
			
			var scrollPosition = new Vector2(_mainScroll.normalizedPosition.x, tab.verticalScrollPosition);
			yield return _mainScroll.SetPositionRoutine(scrollPosition, 0.65f);
		}
		
		#endregion
	}
	
	public void RemoveTab(Transform transform)
	{
		if(_tabs.Count == 1)
			return;
		
		int index = transform.GetSiblingIndex();
		bool isActive = index == _currentTabIndex;
		
		var tab = _tabs[index];
		
		Destroy(tab.gameObject);
		_tabs.RemoveAt(index);
		
		if(isActive)
			OnTabSelect(index);
		
		else
			_currentTabIndex = index % _tabs.Count;
	}
	
	#endregion
	
	public void Quit()
	{
		SaveUserData();
		Application.Quit();
		
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}
	
	public void SaveUserData()
	{
		PlayerPrefs.SetInt("CurrentBookIndex", CurrentBookIndex);
		PlayerPrefs.SetInt("CurrentChapterIndex", CurrentChapterIndex);
		
		PlayerPrefs.SetString("OnStartBibles", "true");
		PlayerPrefs.SetInt("OnStartBibles_Count", BibleInstances.Count);
		
		for(int i = 0; i < BibleInstances.Count; i++)
		{
			int versionIndex = Array.FindIndex(_versions, version => version == BibleInstances[i].version);
			Debug.Log("VERSION INDEX: " + versionIndex);
			
			PlayerPrefs.SetInt($"OnStartBibles_{i}", versionIndex);
		}
		
		if(!_mainScroll) return;
		
		PlayerPrefs.SetFloat("mainScrollPosX", _mainScroll.normalizedPosition.x);
		PlayerPrefs.SetFloat("mainScrollPosY", _mainScroll.normalizedPosition.y);
	}
	
	public class Collection
	{
		public Language language;
		public List<Version> versions;
	}
	
	public class Tab
	{
		public GameObject gameObject;
		public Text txt;
		public int bookIndex;
		public int chapterIndex;
		public float verticalScrollPosition;
		
		public void SetHighlight(bool isHighlighted)
		{
			var highlight = gameObject.transform.GetChild(0);
				highlight.gameObject.SetActive(isHighlighted);
		}
	}
}

public enum ScreenOrientation { Vertical, Horizontal }