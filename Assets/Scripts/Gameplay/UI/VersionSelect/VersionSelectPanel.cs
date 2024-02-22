using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionSelectPanel : MonoBehaviour
{
	[SerializeField] private TMP_Dropdown _languageSelector;
	
	[SerializeField] private VersionSelectItem _optionTemplate;
	[SerializeField] private int _maxRecentCount = 3;
	
	[Space]
	[SerializeField] private GameObject _separator_Recents;
	[SerializeField] private GameObject _separator_Favorites;
	[SerializeField] private GameObject _separator_All;
	
	[Space]
	[SerializeField] private TMP_InputField _searchInput;
	[SerializeField] private RectTransform _searchField;
	[SerializeField] private Vector2 _searchFieldVerticalSizes = new Vector2(50, 80);
	[SerializeField] private GameObject _searchOptions;
	[SerializeField] private VerticalLayoutGroup _body;
	
	[HideInInspector]
	public List<Version>
		recents = new List<Version>(),
		favorites = new List<Version>();
	
	private List<VersionSelectItem>
		_instances_Recent = new List<VersionSelectItem>(),
		_instances_Favorites = new List<VersionSelectItem>(),
		_instances_All = new List<VersionSelectItem>(),
		_instances_SearchMsc = new List<VersionSelectItem>();
	
	private bool _isExpanded_Recent = true;
	private bool _isExpanded_Favorites;
	private bool _isExpanded_All = true;
	
	private int _selectedLanguageIndex;
	
	public RectTransform panel;
	private Vector2 _originalSize;
	
	private Version _onFinishTargetItem;
	private Action _onCancel;
	
	private bool _isSearchModeActive;
	private Coroutine _searchRoutine;
	private string _searchInputValue;
	private bool _searchForSelectedLanguageOnly = true;
	
	GameManager _mgr;
	bool _started;
	
	#region Unity
	
	void Start()
	{
		if(SaveManager.TryLoad<UserData>("VersionSelect", out var userData))
		{
			recents.Clear();
			favorites.Clear();
			
			Array.ForEach(userData.recentVersionIndexes, index => recents.Add(_mgr.Versions[index]));
			Array.ForEach(userData.favoriteVersionIndexes, index => favorites.Add(_mgr.Versions[index]));
		}
		
		_started = true;
		OnEnable();
	}
	
	void OnEnable()
	{
		if(!_started) return;
		
		if(_originalSize == Vector2.zero)
			_originalSize = panel.sizeDelta;
	
		var size = _originalSize;
		var screenWidth = _mgr.ScreenSize.x;
		
		if(size.x > screenWidth)
			size.x = screenWidth;
		
		panel.sizeDelta = size;
		
		UpdateList();
	}
	
	// void OnApplicationQuit()
	void OnDisable()
	{
		if(_isSearchModeActive)
			CancelSearchMode();
		
		_onCancel?.Invoke();
		
		int recentCount = recents.Count;
		int favoriteCount = favorites.Count;
		
		var recentVersionIndexes = new int[recentCount];
		var favoriteVersionIndexes = new int[favoriteCount];
		
		for(int i = 0; i < recentCount; i++)
			recentVersionIndexes[i] = Array.FindIndex(_mgr.Versions, version => version == recents[i]);
		
		for(int i = 0; i < favoriteCount; i++)
			favoriteVersionIndexes[i] = Array.FindIndex(_mgr.Versions, version => version == favorites[i]);
		
		var data = new UserData()
		{
			recentVersionIndexes = recentVersionIndexes,
			favoriteVersionIndexes = favoriteVersionIndexes
		};
		
		SaveManager.Save<UserData>(data, "VersionSelect");
	}
	
	#endregion
	
	public void Init()
	{
		_mgr = GameManager.Instance;
		
		int langCount = _mgr.Languages.Length;
		_mgr.Collections = new GameManager.Collection[langCount];
		
		_languageSelector.ClearOptions();
		var languageSelectorOptions = new List<string>();
		
		for(int i = 0; i < langCount; i++)
		{
			var language = _mgr.Languages[i];
			var versions = new List<Version>();
			
			_mgr.Collections[i] = new GameManager.Collection(){ language = language };
			
			foreach(var version in _mgr.Versions)
			{
				if(version.Language == language)
					versions.Add(version);
			}
			
			_mgr.Collections[i].versions = versions;
			languageSelectorOptions.Add(language.Name);
		}
		
		_languageSelector.AddOptions(languageSelectorOptions);
	}
	
	public void StartSelection(Action<Version> onFinish, Action onCancel = null)
	{
		_onCancel = onCancel;
		
		_searchInput.SetTextWithoutNotify("");
		gameObject.SetActive(true);
		
		StartCoroutine(r());
		IEnumerator r()
		{
			yield return new WaitUntil(()=> _onFinishTargetItem);
			
			onFinish(_onFinishTargetItem);
			_onFinishTargetItem = null;
			
			yield return null;
			gameObject.SetActive(false);
			
			_onCancel = null;
		}
	}
	
	public void OnItemSelected(Version itemVersion) => _onFinishTargetItem = itemVersion;
	
	public void UpdateList()
	{
		destroy(ref _instances_All);
		destroy(ref _instances_Recent);
		destroy(ref _instances_Favorites);
		
		void destroy(ref List<VersionSelectItem> instances)
		{
			for(int i = 0; i < instances.Count; i++)
			{
				if(instances[i])
					Destroy(instances[i].gameObject);
			}
			instances.Clear();
		}
		
		var transform = _optionTemplate.transform;
		var parent = transform.parent;
		
		setupItems(recents, ref _instances_Recent, _isExpanded_Recent, _separator_Recents);
		setupItems(favorites, ref _instances_Favorites, _isExpanded_Favorites, _separator_Favorites);
		
		var collection = _mgr.Collections[_selectedLanguageIndex];
		setupItems(collection.versions, ref _instances_All, _isExpanded_All, _separator_All);
		
		void setupItems(List<Version> versions, ref List<VersionSelectItem> instances, bool isExpanded, GameObject separator)
		{
			bool hasItems = versions.Count > 0;
			separator?.SetActive(hasItems);
			
			if(!hasItems) return;
			
			var transform = separator.transform;
				transform.SetAsLastSibling();
			
			UpdateFoldoutGpx(isExpanded, transform);
			
			foreach(var version in versions)
			{
				var instance = Instantiate(_optionTemplate, parent, false);
					instance.Setup(version, favorites.Contains(version));
					instance.gameObject.SetActive(isExpanded);
				
				instances.Add(instance);
			}
		}
		
		transform.SetAsLastSibling();
		_optionTemplate.gameObject.SetActive(false);
	}
	
	private void UpdateFoldoutGpx(bool isExpanded, Transform target)
	{
		float gpxAngle = isExpanded? 0f: 90f;
		
		var arrow = target.GetChild(0);
			arrow.eulerAngles = Vector3.forward * gpxAngle;
	}
	
	public void OnLanguageSelect(int index = -1)
	{
		if(_isSearchModeActive)
			CancelSearchMode();
		
		if(index >= 0)
			_selectedLanguageIndex = index;
		
		UpdateList();
		
		_isExpanded_All = true;
		_instances_All.ForEach(instance => instance.gameObject.SetActive(_isExpanded_All));
		
		UpdateFoldoutGpx(_isExpanded_All, _separator_All.transform);
	}
	
	public void AddRecents(Version version)
	{
		if(!recents.Contains(version))
			recents.Insert(0, version);
		
		if(recents.Count > _maxRecentCount)
			recents.RemoveAt(recents.Count - 1);
	}
	
	public void ToggleFoldout_Recent() => ToggleFoldout(ref _isExpanded_Recent, ref _instances_Recent, _separator_Recents);
	public void ToggleFoldout_Favorites() => ToggleFoldout(ref _isExpanded_Favorites, ref _instances_Favorites, _separator_Favorites);
	public void ToggleFoldout_All() => ToggleFoldout(ref _isExpanded_All, ref _instances_All, _separator_All);
	
	public void ToggleFoldout(ref bool isExpanded, ref List<VersionSelectItem> instances, GameObject gameObject)
	{
		isExpanded = !isExpanded;
		
		foreach(var instance in  instances)
			instance.gameObject.SetActive(isExpanded);
		
		UpdateFoldoutGpx(isExpanded, gameObject.transform);
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
		
		UpdateList();
	}
	
	#region Search
	
	public void OnSearchInput(string value)
	{
		_searchInputValue = value;
		StartSearchMode();
	}
	
	public void SearchForSelectedLanguageOnly(bool value)
	{
		_searchForSelectedLanguageOnly = value;
		StartSearchMode();
	}
	
	private void StartSearchMode()
	{
		if(_searchRoutine != null)
			StopCoroutine(_searchRoutine);
		
		_searchRoutine = StartCoroutine(r());
		
		IEnumerator r()
		{
			if(!_isSearchModeActive)
			{
				_searchField.sizeDelta = new Vector2(_searchField.sizeDelta.x, _searchFieldVerticalSizes.y);
				_body.padding.top = 25;
				_searchOptions.SetActive(true);
				
				_isSearchModeActive = true;
			}
			
			var genInfo = _mgr.GeneralInfo;
			yield return null;
			
			_separator_Recents.SetActive(false);
			_separator_Favorites.SetActive(false);
			_separator_All.SetActive(false);
			
			_instances_Recent.ForEach(instance => instance.gameObject.SetActive(false));
			_instances_Favorites.ForEach(instance => instance.gameObject.SetActive(false));
			_instances_All.ForEach(instance => instance.gameObject.SetActive(false));
			
			yield return null;
			
			var results = new List<VersionSelectItem>();
			yield return searchIn(_instances_All, results);
			
			_instances_SearchMsc.ForEach(instance => instance.gameObject.SetActive(false));
			
			if(!_searchForSelectedLanguageOnly)
			{
				foreach(var collection in _mgr.Collections)
				{
					if(collection.language == _mgr.Languages[_selectedLanguageIndex])
						continue;
					
					foreach(var version in collection.versions)
					{
						if(_instances_SearchMsc.Exists(instance => instance.Target == version))
							continue;
						
						var instance = Instantiate(_optionTemplate, _optionTemplate.transform.parent, false);
							instance.Setup(version, favorites.Contains(version));
						
						_instances_SearchMsc.Add(instance);
					}
				}
				
				yield return searchIn(_instances_SearchMsc, results);
			}
		}
		
		IEnumerator searchIn(List<VersionSelectItem> instances, List<VersionSelectItem> results)
		{
			foreach(var instance in instances)
			{
				yield return null;
				
				if(results.Contains(instance))
					continue;
				
				string value = _searchInputValue.ToLower();
				var target = instance.Target;
				
				string name = target.Name.ToLower();
				
				if(name.Contains(value))
				{
					onFound(instance);
					continue;
				}
				
				string nameCode = target.NameCode.ToLower();
				
				if(nameCode.Contains(value))
				{
					onFound(instance);
					continue;
				}
				
				if(value.Contains(name))
				{
					onFound(instance);
					continue;
				}
				
				if(value.Contains(nameCode))
				{
					onFound(instance);
					continue;
				}
			}
			
			void onFound(VersionSelectItem instance)
			{
				results.Add(instance);
				instance.gameObject.SetActive(true);
			}
		}
	}
	
	public void CancelSearchMode()
	{
		_searchField.sizeDelta = new Vector2(_searchField.sizeDelta.x, _searchFieldVerticalSizes.x);
		_body.padding.top = 0;
		_searchOptions.SetActive(false);
		
		_isSearchModeActive = false;
		
		for(int i = 0; i < _instances_SearchMsc.Count; i++)
			Destroy(_instances_SearchMsc[i].gameObject);
		
		_instances_SearchMsc.Clear();
	}
	
	#endregion
	
	[System.Serializable]
	public class UserData
	{
		public int[] recentVersionIndexes;
		public int[] favoriteVersionIndexes;
	}
}