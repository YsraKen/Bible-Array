using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class GameManager : MonoBehaviour
{
	[SerializeField] private Language[] _languages;
	[SerializeField] private Version[] _versions;
	
	private Collection[] _collections;
	
	[Space]
	[SerializeField] private BibleUI2 _biblePrefab;
	[SerializeField] private Version[] _onStartBibles;
	
	[Space]
	[SerializeField] private BibleLayout _layout;
	[SerializeField] private TMP_Text _chapterTmp;
	
	[field: SerializeField] public Color JesusWordColor { get; private set; } = Color.red;
	[field: SerializeField] public string VerseCommentLink { get; private set; }
	
	[field: SerializeField] public BibleUI2[] BibleInstances { get; private set; }
	
	[field: SerializeField] public int CurrentBookIndex { get; private set; }
	[field: SerializeField] public int CurrentChapterIndex { get; private set; }
	
	[HideInInspector]
	public List<Version>
		recents = new List<Version>(),
		favorites = new List<Version>();
	
	[SerializeField] private ScrollRect _mainScroll;
	[SerializeField] private float _hudReactionThresholdToMainScrollVelocity = 100f;
	
	[field: SerializeField]
	public Sprite RecentIcon { get; private set; }
	
	[field: SerializeField]
	public Sprite FavoritesIcon { get; private set; }
	
	[SerializeField] private int _maxRecentCount = 3;
	
	// [field: SerializeField]
	// public List<TMP_Dropdown.OptionData> BibleOptionsDefaultList { get; private set; } = new List<TMP_Dropdown.OptionData>();
	
	[Space]
	[SerializeField] private GameObject _versionSelectPanel;
	[SerializeField] private TMP_Dropdown _languageSelector;
	
	private int _selectedLanguageIndex;
	private BibleUI2 _versionSelectTargetBible;
	
	[SerializeField] private VersionSelectItem _versionSelectOptionTemplate;
	
	[Space]
	[SerializeField] private GameObject _verSelectSeparator_Recents;
	[SerializeField] private GameObject _verSelectSeparator_Favorites;
	[SerializeField] private GameObject _verSelectSeparator_All;
	
	private List<VersionSelectItem>
		_verSelectItemInstances_Recent = new List<VersionSelectItem>(),
		_verSelectItemInstances_Favorites = new List<VersionSelectItem>(),
		_verSelectItemInstances_All = new List<VersionSelectItem>();
	
	private bool _verSelectIsExpanded_Recent = true;
	private bool _verSelectIsExpanded_Favorites;
	private bool _verSelectIsExpanded_All = true;
	
	[SerializeField] private Animation _hudAnimation;
	private Vector2 _lastScrollPosition;
	
	private bool _appearHud_OneFrameGate;
	public bool IsHudShown { get; private set; } = true;
	
	[Space]
	[SerializeField] private TMP_Text _popupTitle;
	[SerializeField] private TMP_Text _popup;
	
	[Space]
	[SerializeField] private GameObject _verseOptions;
	
	Vector3 _repositionOffset;
	
	public List<VerseUI2> SelectedVerses { get; private set; } = new List<VerseUI2>(); // or use Dictionary
	
	[SerializeField] private RectTransform _screenSizeRef;
	
	[field: SerializeField]
	public Transform HeadersClampRef { get; private set; }
	
	public Vector2 ScreenSize => _screenSizeRef.rect.size;
	public float DefaultMinWidth { get; private set; }
	
	public UnityEvent<ScreenOrientation> onScreenOrientationChanged;
	public ScreenOrientation ScreenOrientation { get; private set; }
	ScreenOrientation _previousScreenOrientation;
	
	public static GameManager Instance { get; private set; }
	public bool Started { get; private set; }
	
	void Awake()
	{
		Started = false;
		Instance = this;
	}
	
	IEnumerator Start()
	{
		int count = _languages.Length;
		_collections = new Collection[count];
		
		_languageSelector.ClearOptions();
		var languageSelectorOptions = new List<string>();
		
		for(int i = 0; i < count; i++)
		{
			var language = _languages[i];
			var versions = new List<Version>();
			
			_collections[i] = new Collection(){ language = language };
			
			foreach(var version in _versions)
			{
				if(version.Language == language)
					versions.Add(version);
			}
			
			_collections[i].versions = versions;
			languageSelectorOptions.Add(language.Name);
		}
		
		_languageSelector.AddOptions(languageSelectorOptions);
		yield return null;
		
		Array.ForEach(_onStartBibles, osb => CreateBibleInstance(osb));
		yield return null;
		
		DefaultMinWidth = Mathf.Min(ScreenSize.x, ScreenSize.y);
		UpdateBibleContents();
		
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
			_chapterTmp.text = $"<b>{_versions[0].Books[CurrentBookIndex].nickname.ToUpper()}</b> {CurrentChapterIndex + 1}/{_versions[0].Books[CurrentBookIndex].chapters.Length}";
			yield return null;
			
			BibleInstances = _layout.GetComponentsInChildren<BibleUI2>();
			Array.ForEach(BibleInstances, instance => instance.Cover.SetActive(true));
			
			yield return null;
			
			foreach(var instance in BibleInstances)
			{
				instance.UpdateContents();
				instance.UpdateVerticalSize();
			}
			
			yield return null;
			_layout.Repaint();
			
			yield return null;
			Array.ForEach(BibleInstances, instance => instance.Cover.SetActive(false));
		}
	}
	
	public void OpenVersionSelector(BibleUI2 target)
	{
		_versionSelectPanel.SetActive(true);
		_versionSelectTargetBible = target;
		
		UpdateVersionSelectList();
	}
	
	public void OnLanguageSelect(int index)
	{
		_selectedLanguageIndex = index;
		
		UpdateVersionSelectList();
		
		_verSelectIsExpanded_All = true;
		
		foreach(var instance in _verSelectItemInstances_All)
			instance.gameObject.SetActive(_verSelectIsExpanded_All);
		
		UpdateVersionSelectFoldoutGpx(_verSelectIsExpanded_All, _verSelectSeparator_All.transform);
	}
	
	public void UpdateVersionSelectList()
	{
		destroy(ref _verSelectItemInstances_All);
		destroy(ref _verSelectItemInstances_Recent);
		destroy(ref _verSelectItemInstances_Favorites);
		
		void destroy(ref List<VersionSelectItem> instances)
		{
			for(int i = 0; i < instances.Count; i++)
			{
				if(instances[i])
					Destroy(instances[i].gameObject);
			}
			instances.Clear();
		}
		
		var transform = _versionSelectOptionTemplate.transform;
		var parent = transform.parent;
		
		setupItems(recents, ref _verSelectItemInstances_Recent, _verSelectIsExpanded_Recent, _verSelectSeparator_Recents);
		setupItems(favorites, ref _verSelectItemInstances_Favorites, _verSelectIsExpanded_Favorites, _verSelectSeparator_Favorites);
		
		var collection = _collections[_selectedLanguageIndex];
		setupItems(collection.versions, ref _verSelectItemInstances_All, _verSelectIsExpanded_All, _verSelectSeparator_All);
		
		void setupItems(List<Version> versions, ref List<VersionSelectItem> instances, bool isExpanded, GameObject separator)
		{
			bool hasItems = versions.Count > 0;
			separator?.SetActive(hasItems);
			
			if(!hasItems) return;
			
			var transform = separator.transform;
				transform.SetAsLastSibling();
			
			UpdateVersionSelectFoldoutGpx(isExpanded, transform);
			
			foreach(var version in versions)
			{
				var instance = Instantiate(_versionSelectOptionTemplate, parent, false);
					instance.Setup(version, favorites.Contains(version));
					instance.gameObject.SetActive(isExpanded);
				
				instances.Add(instance);
			}
		}
		
		transform.SetAsLastSibling();
		_versionSelectOptionTemplate.gameObject.SetActive(false);
	}
	
	public void OnVersionSelect(Version version)
	{
		if(!_versionSelectTargetBible)
			_versionSelectTargetBible = CreateBibleInstance(version);
			
		else if(_versionSelectTargetBible.version == version)
			return;
		
		_versionSelectTargetBible.SetVersion(version);
		_versionSelectPanel.SetActive(false);
		
		if(recents.Count < _maxRecentCount && !recents.Contains(version))
			recents.Add(version);
		
		StartCoroutine(r());
		IEnumerator r()
		{
			yield return null;
			_versionSelectTargetBible.UpdateContents();
			
			yield return null;
			UpdateBibleContents();
			
			_versionSelectTargetBible = null;
		}
	}
	
	public void ToggleVersionSelectFoldout_Recent() => ToggleVersionSelectFoldout(ref _verSelectIsExpanded_Recent, ref _verSelectItemInstances_Recent, _verSelectSeparator_Recents);
	public void ToggleVersionSelectFoldout_Favorites() => ToggleVersionSelectFoldout(ref _verSelectIsExpanded_Favorites, ref _verSelectItemInstances_Favorites, _verSelectSeparator_Favorites);
	public void ToggleVersionSelectFoldout_All() => ToggleVersionSelectFoldout(ref _verSelectIsExpanded_All, ref _verSelectItemInstances_All, _verSelectSeparator_All);
	
	public void ToggleVersionSelectFoldout(ref bool isExpanded, ref List<VersionSelectItem> instances, GameObject gameObject)
	{
		isExpanded = !isExpanded;
		
		foreach(var instance in  instances)
			instance.gameObject.SetActive(isExpanded);
		
		UpdateVersionSelectFoldoutGpx(isExpanded, gameObject.transform);
	}
	
	private void UpdateVersionSelectFoldoutGpx(bool isExpanded, Transform target)
	{
		float gpxAngle = isExpanded? 0f: 90f;
		
		var arrow = target.GetChild(0);
			arrow.eulerAngles = Vector3.forward * gpxAngle;
	}
	
	public void AddFavoriteVersion(Version version, bool isAdding)
	{
		if(isAdding)
		{
			if(!favorites.Contains(version))
				favorites.Add(version);
		}
		else
		{
			if(favorites.Contains(version))
				favorites.Remove(version);
		}
		
		UpdateVersionSelectList();
	}
	
	public void OnScroll(Vector2 value)
	{
		float velocity = Mathf.Abs(_mainScroll.velocity.y);
		
		if(velocity < _hudReactionThresholdToMainScrollVelocity)
			return;
		
		if(value.y > 0.95f || value.y < 0.05f)
		{
			if(!_appearHud_OneFrameGate)
			{
				_hudAnimation.Play("hudAppear");
				_appearHud_OneFrameGate = true;
			}
			
			IsHudShown = true;
			return;
		}
	
		if(value.y > _lastScrollPosition.y)
		{
			if(!_appearHud_OneFrameGate)
			{
				_hudAnimation.Play("hudAppear");
				// _hudAnimation.gameObject.SetActive(true);
				_appearHud_OneFrameGate = true;
			}
			
			IsHudShown = true;
		}
		
		else
		{
			if(_appearHud_OneFrameGate)
			{
				_hudAnimation.Play("hudDisappear");
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
	}
	
	public void NavigateTo(int bookIndex, int chapterIndex)
	{
		CurrentBookIndex = bookIndex;
		CurrentChapterIndex = chapterIndex;
		
		UpdateBibleContents();
		
		_mainScroll.verticalNormalizedPosition = 1;
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
	
	public void OnVerseSelect(VerseUI2 verseUI/* , Version version */)
	{
		/* if(SelectedVerses.TryFindIndex(selected => selected.Compare(verseUI.Index, version), out int selectedIndex))
		{
			verseUI.OnSelectionHighlight(false);
			SelectedVerses.RemoveAt(selectedIndex);
		}
		else
			SelectedVerses.Add(new SelectedVerse(verseUI, version));
		
		SelectedVerses.ForEach(selected => selected.verseUI.OnSelectionHighlight(true)); */
		
		if(SelectedVerses.Contains(verseUI, out int index))
		{
			verseUI.OnSelectionHighlight(false);
			SelectedVerses.RemoveAt(index);
		}
		else
			SelectedVerses.Add(verseUI);
		
		SelectedVerses.ForEach(selected => selected.OnSelectionHighlight(true));
		_verseOptions.SetActive(SelectedVerses.Count > 0);
	}
	
	public void Reposition_Start(Transform transform) => _repositionOffset = transform.position - Input.mousePosition;
	public void Reposition(Transform transform) => transform.position = Input.mousePosition + _repositionOffset;
	
	public class Collection
	{
		public Language language;
		public List<Version> versions;
	}
	
	/* [System.Serializable]
	public class SelectedVerse
	{
		public VerseUI2 verseUI;
		public Version version;
		
		public SelectedVerse(VerseUI2 verseUI, Version version)
		{
			this.verseUI = verseUI;
			this.version = version;
		}
		
		public bool Compare(int index, Version version) =>	index == verseUI.Index && version == this.version;
	} */
}

public enum ScreenOrientation { Vertical, Horizontal }