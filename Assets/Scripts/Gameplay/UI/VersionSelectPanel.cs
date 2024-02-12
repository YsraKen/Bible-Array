using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionSelectPanel : MonoBehaviour
{
	[SerializeField] private VersionSelectItem _optionTemplate;
	[SerializeField] private int _maxRecentCount = 3;
	
	[Space]
	[SerializeField] private GameObject _separator_Recents;
	[SerializeField] private GameObject _separator_Favorites;
	[SerializeField] private GameObject _separator_All;
	
	[HideInInspector]
	public List<Version>
		recents = new List<Version>(),
		favorites = new List<Version>();
	
	private List<VersionSelectItem>
		_instances_Recent = new List<VersionSelectItem>(),
		_instances_Favorites = new List<VersionSelectItem>(),
		_instances_All = new List<VersionSelectItem>();
	
	private bool _isExpanded_Recent = true;
	private bool _isExpanded_Favorites;
	private bool _isExpanded_All = true;
	
	private int _selectedLanguageIndex;
	
	public RectTransform panel;
	private Vector2 _originalSize;
	
	private Version _onFinishTargetItem;
	
	bool _started;
	
	#region Unity
	
	void Start()
	{
		if(SaveManager.TryLoad<UserData>("VersionSelect", out var userData))
		{
			recents.Clear();
			favorites.Clear();
			
			var mgr = GameManager.Instance;
			
			Array.ForEach(userData.recentVersionIndexes, index => recents.Add(mgr.Versions[index]));
			Array.ForEach(userData.favoriteVersionIndexes, index => favorites.Add(mgr.Versions[index]));
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
		var screenWidth = GameManager.Instance.ScreenSize.x;
		
		if(size.x > screenWidth)
			size.x = screenWidth;
		
		panel.sizeDelta = size;
		
		UpdateList();
	}
	
	// void OnApplicationQuit()
	void OnDisable()
	{
		int recentCount = recents.Count;
		int favoriteCount = favorites.Count;
		
		var recentVersionIndexes = new int[recentCount];
		var favoriteVersionIndexes = new int[favoriteCount];
		
		var mgr = GameManager.Instance;
		
		for(int i = 0; i < recentCount; i++)
			recentVersionIndexes[i] = Array.FindIndex(mgr.Versions, version => version == recents[i]);
		
		for(int i = 0; i < favoriteCount; i++)
			favoriteVersionIndexes[i] = Array.FindIndex(mgr.Versions, version => version == favorites[i]);
		
		var data = new UserData()
		{
			recentVersionIndexes = recentVersionIndexes,
			favoriteVersionIndexes = favoriteVersionIndexes
		};
		
		SaveManager.Save<UserData>(data, "VersionSelect");
	}
	
	#endregion
	
	public void StartSelection(Action<Version> onFinish)
	{
		gameObject.SetActive(true);
		
		StartCoroutine(r());
		IEnumerator r()
		{
			yield return new WaitUntil(()=> _onFinishTargetItem);
			
			onFinish(_onFinishTargetItem);
			_onFinishTargetItem = null;
			
			yield return null;
			gameObject.SetActive(false);
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
		
		var collection = GameManager.Instance.Collections[_selectedLanguageIndex];
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
	
	public void OnLanguageSelect(int index)
	{
		_selectedLanguageIndex = index;
		
		UpdateList();
		
		_isExpanded_All = true;
		
		foreach(var instance in _instances_All)
			instance.gameObject.SetActive(_isExpanded_All);
		
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
	
	[System.Serializable]
	public class UserData
	{
		public int[] recentVersionIndexes;
		public int[] favoriteVersionIndexes;
	}
}