using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
	#region Variables and Properties
	
	public Collection[] Collections { get; private set; }
	
	[Space]
	[SerializeField] private BibleUI2 _biblePrefab;
	
	[SerializeField, FormerlySerializedAs("_onStartBibles")]
	private Version[] _onStartBiblesDefault;
	
	[field: SerializeField]
	public GeneralInformation GeneralInfo { get; private set; }
	
	[field: SerializeField]
	public MarkManager MarkManager { get; private set; }
	
	public Version[] Versions => GeneralInfo.allVersions;
	public Language[] Languages => GeneralInfo.allLanguages;
	
	[field: SerializeField] public Color JesusWordColor { get; private set; } = Color.red;
	[field: SerializeField] public string VerseCommentLink { get; private set; }
	
	[field: SerializeField]
	public List<BibleUI2> BibleInstances { get; private set; } = new List<BibleUI2>();
	
	public int CurrentBookIndex { get; private set; }
	public int CurrentChapterIndex { get; private set; }
	
	public ChapterUserData ChapterUserData { get; private set; }
	
	[Space]
	[SerializeField] private GameObject _tabTemplate;
	[SerializeField] private ScrollRect _tabsScroll;
	
	[SerializeField] private List<Tab> _tabs = new List<Tab>();
	private int _currentTabIndex;
	private Coroutine _tabRoutine;
	
	[SerializeField] private ScrollRect _mainScroll;
	[SerializeField] private float _hudReactionThresholdToMainScrollVelocity = 100f;
	
	Coroutine _hudReactionRoutine;
	bool _isHudReacting;
	
	// for later saving data: Unity annoyingly destroy "_mainScroll" even before calling "this" script's "OnDisable" method
	Vector2 _scrollNormalizedPositionCache;
	
	[Space]
	[SerializeField] private Animation _navigator;
	
	bool _isNavigatorExpanded = true;
	
	[SerializeField] private Button _upButton;
	[SerializeField] private Button _downButton;
	[SerializeField] private Button _leftButton;
	[SerializeField] private Button _rightButton;
	
	[SerializeField] private Button _prevButton;
	[SerializeField] private Button _nextButton;
	
	private Coroutine mainScrollAutoScrollerRoutine;
	
	[Space]
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
	[SerializeField] private ScrollRect _popupScroll;
	
	[Space]
	[SerializeField] private VerseOptionsPanel _verseOptions;
	
	Vector3 _repositionOffset;
	
	public List<VerseUI2> SelectedVerses { get; private set; } = new List<VerseUI2>(); // or use Dictionary
	public Action onVerseSelectUpdate;
	
	[SerializeField] private RectTransform _screenSizeRef;
	
	[field: SerializeField]
	public Transform HeadersClampRef { get; private set; }
	
	[SerializeField] private GameObject _quitPanel;
	[SerializeField] private TMP_Text _sessionName;
	
	public Vector2 ScreenSize => _screenSizeRef.rect.size;
	public float DefaultMinWidth { get; private set; }
	
	public UnityEvent<ScreenOrientation> onScreenOrientationChanged;
	public ScreenOrientation ScreenOrientation { get; private set; }
	ScreenOrientation _previousScreenOrientation;
	
	public static GameManager Instance { get; private set; }
	
	public bool Started { get; private set; }
	private bool _userDataSaved;
	
	#endregion
	
	#region Unity
	
	void Awake()
	{
		if(!AppEntry.IsLoaded)
		{
			SceneManager.LoadScene("AppEntry");
			return;
		}
	
		MainContentLoadingOverlay.SetActive(true);
		_navigator.gameObject.SetActive(false);
		
		MarkManager.LoadData();
		
		Started = false;
		Instance = this;
	}
	
	IEnumerator Start()
	{
		int langCount = Languages.Length;
		Collections = new Collection[langCount];
		
		_languageSelector.ClearOptions();
		var languageSelectorOptions = new List<string>();
		
		for(int i = 0; i < langCount; i++)
		{
			var language = Languages[i];
			var versions = new List<Version>();
			
			Collections[i] = new Collection(){ language = language };
			
			foreach(var version in Versions)
			{
				if(version.Language == language)
					versions.Add(version);
			}
			
			Collections[i].versions = versions;
			languageSelectorOptions.Add(language.Name);
		}
		
		_languageSelector.AddOptions(languageSelectorOptions);
		yield return null;
		
		yield return LoadOnstartDefaultBibleData();
		yield return AddOnStartTabs();
		
		CurrentBookIndex = _tabs[_currentTabIndex].bookIndex;
		CurrentChapterIndex = _tabs[_currentTabIndex].chapterIndex;
		
		UpdateCurrentBookAndChapterData();
		UpdateBibleContents();
		
		yield return new WaitWhile(()=> IsLoadingContent);
		yield return InitializeScrollPositionings();
		
		DefaultMinWidth = Mathf.Min(ScreenSize.x, ScreenSize.y);
		UpdateSessionNameUI();
		
		yield return null;
		
		UpdateNavButtons();
		_navigator.gameObject.SetActive(true);
		
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
			_quitPanel.SetActive(!_quitPanel.activeSelf);
	}
	
	void OnApplicationQuit() => SaveUserData();
	
	void OnDisable()
	{
		if(!_userDataSaved)
			SaveUserData();
	}
	
	#endregion
	
	#region Initializations
	
	IEnumerator LoadOnstartDefaultBibleData()
	{
		var sessionData = SessionSelect.LoadedData;
		
		if(sessionData == null)
		{
			Array.ForEach(_onStartBiblesDefault, osb => CreateBibleInstance(osb));
			yield break;
		}
		
		if(sessionData.versions.IsNullOrEmpty())
		{
			Array.ForEach(_onStartBiblesDefault, osb => CreateBibleInstance(osb));
			yield break;
		}
		
		int onStartBibleCount = sessionData.versions.Length;
		
		for(int i = 0; i < onStartBibleCount; i++)
		{
			int versionIndex = sessionData.versions[i];
			var version = Versions[versionIndex];
			
			CreateBibleInstance(version);
			yield return null;
		}
	}
	
	IEnumerator AddOnStartTabs()
	{
		var sessionData = SessionSelect.LoadedData;
		
		if(sessionData == null)
		{
			addDefaultTab();
			yield break;
		}
		
		if(sessionData.tabs.IsNullOrEmpty())
		{
			addDefaultTab();
			yield break;
		}
		
		void addDefaultTab()
		{
			AddTab(0, 0);
			
			var newTab = _tabs[_tabs.Count -1];
				newTab.scrollPosition = Vector2.up;
		}
		
		for(int i = 0; i < sessionData.tabs.Length; i++)
		{
			var data = sessionData.tabs[i];
			
			AddTab(data.bookIndex, data.chapterIndex, data.scrollPosition, false);
		}
		
		yield return null;
		
		int currentIndex = sessionData.currentTabIndex;
		OnTabSelect(currentIndex);
		
		_tabsScroll.horizontalNormalizedPosition = Mathf.InverseLerp(0, _tabs.Count - 1, currentIndex);
	}
	
	void UpdateCurrentBookAndChapterData()
	{
		ChapterUserData = SaveManager.Load<ChapterUserData>(GetChapterUserDataKey());
		
		if(ChapterUserData == null)
			ChapterUserData = new ChapterUserData();
	}
	
	IEnumerator InitializeScrollPositionings()
	{
		if(SessionSelect.LoadedData == null)
			yield break;
		
		var tabsData = SessionSelect.LoadedData.tabs;
		
		for(int i = 0; i < _tabs.Count; i++)
		{
			if(tabsData.IsInsideRange(i))
				_tabs[i].scrollPosition = tabsData[i].scrollPosition;
		}
		
		_mainScroll.normalizedPosition = Vector2.up;
		yield return null;
		
		var scrollPosition = _tabs[_currentTabIndex].scrollPosition;			
		yield return _mainScroll.SetPositionRoutine(scrollPosition, 0.65f);
	}
	
	void UpdateSessionNameUI()
	{
		if(SessionSelect.LoadedData == null)
		{
			_sessionName.gameObject.SetActive(false);
			return;
		}
	
		_sessionName.text = SessionSelect.LoadedData.name;
		
		if(string.IsNullOrEmpty(_sessionName.text))
			_sessionName.text = $"Session\n<i><size={_sessionName.fontSize * 0.75f}>{SessionSelect.LoadedData.dateCreated}</size></i>";
	}
	
	BibleUI2 CreateBibleInstance(Version version)
	{
		var instance = Instantiate(_biblePrefab, _layout.transform, false);
			instance.version = version;
		
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
	
	#endregion
	
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
	
	#region Scroll Navigation
	
	public void OnScroll(Vector2 value)
	{
		_scrollNormalizedPositionCache = value;
		UpdateNavButtons();
		
		if(_isHudReacting) return;
		
		float velocity = Mathf.Abs(_mainScroll.velocity.y);
		
		if(velocity < _hudReactionThresholdToMainScrollVelocity)
			return;
		
		if(value.y > 0.95f || value.y < 0.05f)
		{
			if(!_appearHud_OneFrameGate)
			{
				string clip = ScreenOrientation == ScreenOrientation.Horizontal? "hudAppearHorizontal": "hudAppear";
				Array.ForEach(_hudAnimations, animation => animation.Play(clip));
				
				// SetNavigatorFoldout(true);
				
				restartHudReaction();
				_appearHud_OneFrameGate = true;
			}
			
			IsHudShown = true;
			return;
		}
	
		if(value.y > _lastScrollPosition.y)
		{
			if(!_appearHud_OneFrameGate)
			{
				string clip = ScreenOrientation == ScreenOrientation.Horizontal? "hudAppearHorizontal": "hudAppear";
				Array.ForEach(_hudAnimations, animation => animation.Play(clip));
				
				// SetNavigatorFoldout(true);
				
				restartHudReaction();
				_appearHud_OneFrameGate = true;
			}
			
			IsHudShown = true;
		}
		
		else
		{
			if(_appearHud_OneFrameGate)
			{
				Array.ForEach(_hudAnimations, animation => animation.Play("hudDisappear"));
				
				SetNavigatorFoldout(false);
				
				restartHudReaction();
				_appearHud_OneFrameGate = false;
			}
			
			IsHudShown = false;
		}
		
		_lastScrollPosition = value;
		
		void restartHudReaction()
		{
			if(_hudReactionRoutine != null)
				StopCoroutine(_hudReactionRoutine);
			
			_hudReactionRoutine = StartCoroutine(r());
			IEnumerator r()
			{
				_isHudReacting = true;
				yield return new WaitForSeconds(0.5f);
				_isHudReacting = false;
			}
		}
	}
	
	public void ToggleNavigatorFoldout() => SetNavigatorFoldout(!_isNavigatorExpanded);
	
	void SetNavigatorFoldout(bool isExpanded)
	{
		if(_isNavigatorExpanded == isExpanded)
			return;
		
		_isNavigatorExpanded = isExpanded;
		
		if(!_navigator.gameObject.activeSelf)
			return;
		
		string animation = _isNavigatorExpanded? "expand": "collapse";
		_navigator.Play(animation);
		
		if(!_isNavigatorExpanded)
			return;
	
		StartCoroutine(oneFrameDelay());
		IEnumerator oneFrameDelay()
		{
			yield return null;
			Reposition_End((RectTransform) _navigator.transform);
		}
	}
	
	private void UpdateNavButtons()
	{
		_upButton.interactable = _scrollNormalizedPositionCache.y < 0.95f;
		_downButton.interactable = _scrollNormalizedPositionCache.y > 0.05f;
		
		bool isLeftActive = _scrollNormalizedPositionCache.x > 0.05f;
		bool isRightActive = _scrollNormalizedPositionCache.x < 0.95f;
		
		_leftButton.interactable = isLeftActive;
		_rightButton.interactable = isRightActive;
	
		_prevButton.interactable = isLeftActive;
		_nextButton.interactable = isRightActive;
	}
	
	public void NavigateScroll(int direction)
	{
		direction = Mathf.Clamp(direction, -1, 1);
		
		int count = BibleInstances.Count;
		
		int currentBibleIndex = Mathf.RoundToInt(Mathf.Lerp(0, count - 1, _mainScroll.horizontalNormalizedPosition));
			currentBibleIndex += direction;
			currentBibleIndex = currentBibleIndex % BibleInstances.Count;
		
		float normalizedPosition = Mathf.Clamp01((float) currentBibleIndex / ((float) count - 1));
		
		if(mainScrollAutoScrollerRoutine != null)
			StopCoroutine(mainScrollAutoScrollerRoutine);
		
		mainScrollAutoScrollerRoutine = StartCoroutine(r());
		IEnumerator r()
		{
			var position = new Vector2(normalizedPosition, _mainScroll.normalizedPosition.y);
			yield return _mainScroll.SetPositionRoutine(position, 0.5f);
		}
	}
	
	public void NavigateHorizontalScroll(float normalizedValue)
	{
		normalizedValue = Mathf.Clamp01(normalizedValue);
		
		if(mainScrollAutoScrollerRoutine != null)
			StopCoroutine(mainScrollAutoScrollerRoutine);
		
		mainScrollAutoScrollerRoutine = StartCoroutine(r());
		IEnumerator r()
		{
			var position = new Vector2(normalizedValue, _mainScroll.normalizedPosition.y);
			yield return _mainScroll.SetPositionRoutine(position, 0.5f);
		}
	}
	
	public void NavigatVerticalScroll(float normalizedValue)
	{
		normalizedValue = Mathf.Clamp01(normalizedValue);
		
		if(mainScrollAutoScrollerRoutine != null)
			StopCoroutine(mainScrollAutoScrollerRoutine);
		
		mainScrollAutoScrollerRoutine = StartCoroutine(r());
		IEnumerator r()
		{
			var position = new Vector2(_mainScroll.normalizedPosition.x, normalizedValue);
			yield return _mainScroll.SetPositionRoutine(position, 0.5f);
		}
	}
	
	#endregion
	
	#region Bible Body
	
	public void IterateChapter(int dir)
	{
		if(ChapterUserData != null)
			SaveManager.Save(ChapterUserData, GetChapterUserDataKey());
		
		dir = Mathf.Clamp(dir, -1, 1);
		CurrentChapterIndex += dir;
		
		var version = Versions[0];
		
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
		
		UpdateCurrentBookAndChapterData();
		UpdateBibleContents();
		UpdateTabText();
		
		_mainScroll.verticalNormalizedPosition = 1;
	}
	
	public void NavigateTo(int bookIndex, int chapterIndex)
	{
		// if(!Started) return;
		
		if(ChapterUserData != null)
			SaveManager.Save(ChapterUserData, GetChapterUserDataKey());
		
		CurrentBookIndex = bookIndex;
		CurrentChapterIndex = chapterIndex;
		
		_verseOptions.OnClose();
		
		UpdateCurrentBookAndChapterData();
		UpdateBibleContents();
		
		#region TABS
		var currentTab = _tabs[_currentTabIndex];
		
		_tabs.ForEach(tab => tab.SetHighlight(false));
		currentTab.SetHighlight(true);
		
		if(currentTab.bookIndex == bookIndex && currentTab.chapterIndex == chapterIndex)
			return;
		
		currentTab.bookIndex = bookIndex;
		currentTab.chapterIndex = chapterIndex;
		
		UpdateTabText();
		#endregion
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
		
		Reposition_End((RectTransform) transform);
		_popupScroll.verticalNormalizedPosition = 1f;
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
		
		bool showOptions = SelectedVerses.Count > 0;
		_verseOptions.gameObject.SetActive(showOptions);
		
		if(showOptions)
			_verseOptions.CheckRemoveHighlightOption();
		
		onVerseSelectUpdate?.Invoke();
	}
	
	public void Reposition_Start(Transform transform) => _repositionOffset = transform.position - Input.mousePosition;
	public void Reposition(Transform transform) => transform.position = Input.mousePosition + _repositionOffset;
	
	public void Reposition_End(RectTransform rTransform)
	{
		var rectSize = rTransform.rect.size;
		// var screenSize = ScreenSize * 2f;
		
		var clampedPosition = new Vector3
		(
			Mathf.Clamp(rTransform.position.x, rectSize.x, Screen.width - rectSize.x),
			Mathf.Clamp(rTransform.position.y, rectSize.y, Screen.height - rectSize.y)
		);
		
		rTransform.position = clampedPosition;
	}
	
	public BibleUI2 GetActiveBible()
	{
		int index = Mathf.RoundToInt(Mathf.Lerp(0, BibleInstances.Count - 1, _mainScroll.horizontalNormalizedPosition));
		return BibleInstances[index];
	}
	
	#endregion
	
	#region Tabs
	
	public void AddTab(int bookIndex, int chapterIndex, Vector2 scrollPosition = default, bool ping = true)
	{
		_tabTemplate.SetActive(true);
		
		var templateT = _tabTemplate.transform;
		var tab = Instantiate(_tabTemplate, templateT.parent, false);
		
		var txt = tab.GetComponentInChildren<Text>();
			txt.text = $"{GeneralInfo.bookChapterVerseInfos[bookIndex].name} {chapterIndex + 1}";
		
		if(scrollPosition == default)
			scrollPosition = new Vector2(_mainScroll.normalizedPosition.x, 1f);
		
		var info = new Tab()
		{
			gameObject = tab,
			txt = txt,
			bookIndex = bookIndex,
			chapterIndex = chapterIndex,
			scrollPosition = scrollPosition
		};
		
		_tabs.Add(info);
		_currentTabIndex = _tabs.Count - 1;
		
		_tabTemplate.SetActive(false);
		templateT.SetAsLastSibling();
		
		if(!Started) return;
		
		if(ping)
			StartCoroutine(r());
		else
			_mainScroll.normalizedPosition = scrollPosition;
		
		IEnumerator r()
		{
			yield return null;
			
			_tabsScroll.horizontalNormalizedPosition = 1f;
			_mainScroll.normalizedPosition = scrollPosition;
			
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
		if(index == _currentTabIndex)
			return;
	
		if(_tabRoutine != null)
			StopCoroutine(_tabRoutine);
		
		_tabRoutine = StartCoroutine(r());
		
		IEnumerator r()
		{
			_tabs[_currentTabIndex].scrollPosition = GetTabScrollPosition(_currentTabIndex);
			yield return null;
			
			_currentTabIndex = index % _tabs.Count;
			var tab = _tabs[_currentTabIndex];
			
			NavigateTo(tab.bookIndex, tab.chapterIndex);
			yield return null;
			
			_tabs.ForEach(tab => tab.SetHighlight(false));
			_tabs[_currentTabIndex].SetHighlight(true);
			
			yield return new WaitWhile(() => IsLoadingContent);
			_mainScroll.normalizedPosition = tab.scrollPosition;
		}
	}
	
	private Vector2 GetTabScrollPosition(int tabIndex)
	{
		if(!_tabs.IsInsideRange(_currentTabIndex))
			return new Vector2(_scrollNormalizedPositionCache.x, 1);
	
		// Snap the scroll position to the nearest bible instance index
		int biblesMaxIndex = BibleInstances.Count - 1;
		int nearestBibleIndex = Mathf.RoundToInt(Mathf.Lerp(0, biblesMaxIndex, _scrollNormalizedPositionCache.x));
		
		var scrollPosition = _scrollNormalizedPositionCache;
			scrollPosition.x = Mathf.InverseLerp(0, biblesMaxIndex, nearestBibleIndex);
		
		return scrollPosition;
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
			OnTabSelect(index % _tabs.Count);
		
		/* if(isActive)
			OnTabSelect(index);
		
		else
			_currentTabIndex = index % _tabs.Count; */
	}
	
	#endregion
	
	#region MSC
	
	public void Home() => SceneManager.LoadScene("SessionSelect");
	
	public void Settings() {}
	
	public void Quit()
	{
		Application.Quit();
		
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}
	
	public void SaveUserData()
	{
		if(!Started) return;
		
		var sessionData = SessionSelect.LoadedData;
		string dateTimeNow = DateTime.Now.ToString();
		
		if(sessionData == null)
		{
			sessionData = new Session();
			sessionData.dateCreated = dateTimeNow;
		}
	
		#region Versions
		
		int bibleInstancesCount = BibleInstances.Count;
		sessionData.versions = new int[bibleInstancesCount];
		
		for(int i = 0; i < bibleInstancesCount; i++)
			sessionData.versions[i] = Array.FindIndex(Versions, version => version == BibleInstances[i].version);
		
		sessionData.dateModified = dateTimeNow;
		
		#endregion
		
		#region Tabs
		
		_tabs[_currentTabIndex].scrollPosition = GetTabScrollPosition(_currentTabIndex);
		
		int tabsCount = _tabs.Count;
		sessionData.tabs = new Tab.UserData[tabsCount];
		
		for(int i = 0; i < tabsCount; i++)
			sessionData.tabs[i] = new Tab.UserData(_tabs[i]);
		
		sessionData.currentTabIndex = _currentTabIndex;
		
		#endregion
		
		SessionSelect.SaveCurrentSessionData(sessionData);
		
		SaveManager.Save(ChapterUserData, GetChapterUserDataKey());
		MarkManager.SaveData();
		
		_userDataSaved = true;
	}
	
	#endregion
	
	#region Info and Data
	
	public class Collection
	{
		public Language language;
		public List<Version> versions;
	}
	
	public string GetChapterUserDataKey(int bookIndex = -1, int chapterIndex = -1)
	{
		if(bookIndex < 0)
			bookIndex = CurrentBookIndex;
		
		if(chapterIndex < 0)
			chapterIndex = CurrentChapterIndex;
		
		return $"ChapterUserData_b{bookIndex}_c{chapterIndex}";
	}
	
	#endregion
}

public enum ScreenOrientation { Vertical, Horizontal }

[Serializable]
public class Tab
{
	public GameObject gameObject;
	public Text txt;
	public int bookIndex;
	public int chapterIndex;
	public Vector2 scrollPosition = Vector2.up;
	
	public void SetHighlight(bool isHighlighted)
	{
		var highlight = gameObject.transform.GetChild(0);
			highlight.gameObject.SetActive(isHighlighted);
	}
	
	[Serializable]
	public class UserData
	{
		public int bookIndex;
		public int chapterIndex;
		public Vector2 scrollPosition = Vector2.up;
		
		public UserData(Tab tab)
		{
			bookIndex = tab.bookIndex;
			chapterIndex = tab.chapterIndex;
			scrollPosition = tab.scrollPosition;
		}
	}
}

[Serializable]
public class ChapterUserData
{
	public List<VerseData> verseDatas = new List<VerseData>();
	
	public VerseData this[int index]
	{
		get => verseDatas[index];
		set => verseDatas[index] = value;
	}
	
	[Serializable]
	public struct VerseData
	{
		// Target referencing
		public int index;
		public int versionIndex;
		
		// Customization Datas
		public int markIndex;
	}
}