using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SessionSelect : MonoBehaviour
{
	public static Session CurrentSession { get; private set; }
	
	public List<Session> allSessions = new List<Session>();
	public List<LabelInfo> labels = new List<LabelInfo>();
	
	private static int _previousSessionIndex = -1;
	public static Session LoadedData { get; private set; }
	
	[Space]
	[SerializeField] private SessionItem _itemTemplate;
	
	[Space]
	[SerializeField] private SessionLabelItem _labelItemTemplate;
	[SerializeField] private SessionLabelItem _allLabelItem;
	[SerializeField] private Transform _addLabelButton;
	
	[Space]
	[SerializeField] private Transform _previousSessionHeader;
	[SerializeField] private Transform _otherSessionsHeader;
	
	bool _hasPreviousSession;
	bool _hasOtherSessions;
	
	[Space]
	[SerializeField] private GameObject _editScreen;
	[SerializeField] private TMP_InputField _editInputField;
	
	[Space]
	[SerializeField] private GameObject _newSessionScreen;
	[SerializeField] private GameObject _loadScreenOverlay;
	
	[Space]
	[SerializeField] private GameObject _multiSelectOptions;
	[SerializeField] private GameObject _multiselectLabelSelect;
	[SerializeField] private Dropdown _labelAsignDropdown;
	
	[field: SerializeField]
	public float MultiSelectHoldTrigger { get; private set; }
	public bool IsMultiSelecting { get; private set; }
	
	private List<SessionItem> _itemInstances = new List<SessionItem>();
	private List<SessionLabelItem> _labelInstances = new List<SessionLabelItem>();
	
	private List<int> _multiSelectedItems = new List<int>();
	
	private int _targetEditIndex;
	public string editInput { get; set; }
	public string newSessionInput { get; set; }
	
	public const string SAVE_KEY = "SessionSelect";
	
	public static SessionSelect Instance { get; private set; }
	
	void Awake()
	{
		Instance = this;
		
		if(SaveManager.TryLoad<UserData>(SAVE_KEY, out UserData data))
		{
			allSessions.Clear();
			labels.Clear();
			
			Array.ForEach(data.allSessions, session => allSessions.Add(session));
			Array.ForEach(data.labels, label => labels.Add(label));
			
			_previousSessionIndex = data.previousSessionIndex;
		}
	}
	
	void Start()
	{
		#region Main Session Items
		
		_itemTemplate.gameObject.SetActive(true);
		
		var itemParent = _itemTemplate.transform.parent;
		int index = 0;
		
		foreach(var session in allSessions)
		{
			var item = Instantiate(_itemTemplate, itemParent, false);
				item.Init(index ++, session);
			
			_itemInstances.Add(item);
		}
		
		_itemTemplate.gameObject.SetActive(false);
		
		_hasPreviousSession = _itemInstances.TryGetElement(_previousSessionIndex, out var instance);	
		_hasOtherSessions = _hasPreviousSession && _itemInstances.Count > 1;
		
		_previousSessionHeader.gameObject.SetActive(_hasPreviousSession);
		_otherSessionsHeader.gameObject.SetActive(_hasOtherSessions);
		
		if(_hasPreviousSession)
		{
			_previousSessionHeader.SetAsFirstSibling();
			int prevSessionTargetChildIndex = 1; // _previousSessionHeader.GetSiblingIndex() + 1;
			
			instance.transform.SetSiblingIndex(prevSessionTargetChildIndex);
			_otherSessionsHeader.SetSiblingIndex(prevSessionTargetChildIndex + 1);
		}
		
		#endregion
	
		#region Labels
		
		_labelItemTemplate.gameObject.SetActive(true);
		
		var labelItemParent = _labelItemTemplate.transform.parent;
		int labelIndex = 0;
		
		foreach(var label in labels)
		{
			var item = Instantiate(_labelItemTemplate, labelItemParent, false);
				item.Init(labelIndex ++, label.name);
			
			_labelInstances.Add(item);
		}
		
		_labelItemTemplate.gameObject.SetActive(false);
		
		#endregion
	}
	
	void OnDisable()
	{
		int sessionCount = allSessions.Count;
		int labelCount = labels.Count;
		
		var userData = new UserData
		(
			new Session[sessionCount],
			_previousSessionIndex,
			new LabelInfo[labelCount]
		);
		
		for(int i = 0; i < sessionCount; i++)
			userData.allSessions[i] = allSessions[i];
		
		for(int i = 0; i < labelCount; i++)
			userData.labels[i] = labels[i];
		
		SaveManager.Save(userData, SAVE_KEY);
	}
	
	public void OnSessionSelect(int index)
	{
		if(IsMultiSelecting)
			return;
		
		LoadedData = allSessions[index];
		_previousSessionIndex = index;
		
		LoadMainScene();
	}
	
	#region Edit
	
	public void EditSession(int index)
	{
		if(IsMultiSelecting)
			return;
		
		_targetEditIndex = index;
		
		_editInputField.SetTextWithoutNotify(allSessions[index].name);
		_editScreen.SetActive(true);
	}
	
	public void OnEditDelete()
	{
		while(_itemInstances.TryFindIndex(instance => instance.Index == _targetEditIndex, out int instanceIndex))
		{
			Destroy(_itemInstances[instanceIndex].gameObject);
			_itemInstances.RemoveAt(instanceIndex);
		}
		
		allSessions.RemoveAt(_targetEditIndex);
		
		_editScreen.SetActive(false);
	}
	
	public void OnEditSave()
	{
		var session = allSessions[_targetEditIndex];
			session.name = editInput;
		
		if(_itemInstances.TryFind(instance => instance.Index == _targetEditIndex, out var instance))
			instance.UpdateName(session);
	
		allSessions[_targetEditIndex] = session;	
		_editScreen.SetActive(false);
	}
	
	#endregion
	
	#region Create
	
	public void NewSession()
	{
		_previousSessionIndex = -1;
		_newSessionScreen.SetActive(true);
	}
	
	public void OnNewSessionSubmit()
	{
		var session = CreateSessionData(newSessionInput);
		allSessions.Add(session);
		
		LoadedData = session;
		_previousSessionIndex = allSessions.Count - 1;
		
		LoadMainScene();
	}
	
	public static Session CreateSessionData(string name = null)
	{
		string dateTimeNow = DateTime.Now.ToString();
		
		var data = new Session()
		{
			name = name,
			dateCreated = dateTimeNow,
			dateModified = dateTimeNow
		};
		
		return data;
	}
	
	#endregion
	
	#region Labels
	
	public void OnAllLabelsToggle()
	{
		_previousSessionHeader.gameObject.SetActive(_hasPreviousSession);
		_otherSessionsHeader.gameObject.SetActive(_hasOtherSessions);
		
		_labelInstances.ForEach(instance => instance.SetHighlight(false));
		_allLabelItem.SetHighlight(true);
	}
	
	public void CreateNewLabel()
	{
		var label = new LabelInfo(){ name = "New Label" };
			labels.Add(label);
		
		var instance = Instantiate(_labelItemTemplate, _labelItemTemplate.transform.parent, false);
			instance.Init(labels.Count - 1, label.name);
			instance.gameObject.SetActive(true);
		
		_labelInstances.Add(instance);
		_addLabelButton.SetAsLastSibling();
	}
	
	public void OnLabelSelect(int index)
	{
		_previousSessionHeader.gameObject.SetActive(false);
		_otherSessionsHeader.gameObject.SetActive(false);
		
		foreach(var label in labels)
			Array.ForEach(label.allSessionsIndex, sessionIndex => _itemInstances[sessionIndex].gameObject.SetActive(false));
		
		Array.ForEach(labels[index].allSessionsIndex, sessionIndex => _itemInstances[sessionIndex].gameObject.SetActive(false));
		
		for(int i = 0; i < _labelInstances.Count; i++)
			_labelInstances[i].SetHighlight(i == index);
		
		_allLabelItem.SetHighlight(false);
	}
	
	public void OnLabelEdit(int index, string name)
	{
		var label = labels[index];
			label.name = name;
		
		labels[index] = label;
	}
	
	public void OnLabelDelete(int index)
	{
		var label = labels[index];
		var instance = _labelInstances[index];
		
		Destroy(instance.gameObject);
		
		labels.RemoveAt(index);
		_labelInstances.RemoveAt(index);
	}
	
	#endregion
	
	#region Multi-selection Mode
	
	public void EnterMultiSelectMode()
	{
		_multiSelectOptions.SetActive(true);
		
		_previousSessionHeader.gameObject.SetActive(false);
		_otherSessionsHeader.gameObject.SetActive(false);
		
		_itemInstances.ForEach(instance => instance.MultiSelectToggle.gameObject.SetActive(true));
		
		IsMultiSelecting = true;
	}
	
	public void ExitMultiSelectMode()
	{
		_multiSelectOptions.SetActive(false);
		
		_previousSessionHeader.gameObject.SetActive(_hasPreviousSession);
		_otherSessionsHeader.gameObject.SetActive(_hasOtherSessions);
		
		_itemInstances.ForEach(instance => instance.MultiSelectToggle.gameObject.SetActive(false));
		
		IsMultiSelecting = false;
	}
	
	public void OnMultiselectAddItem(Toggle toggle)
	{
		int index = toggle.transform.GetSiblingIndex();
		
		if(toggle.isOn)
		{
			if(!_multiSelectedItems.Contains(index))
				_multiSelectedItems.Add(index);
		}
		else
		{
			if(_multiSelectedItems.Contains(index))
				_multiSelectedItems.Remove(index);
		}
	}
	
	public void MultiSelect_All(bool isOn)
	{
		_itemInstances.ForEach(instance => instance.MultiSelectToggle.isOn = isOn);
	}
	
	public void MultiSelect_Delete()
	{
		foreach(int index in _multiSelectedItems)
		{
			var itemInstance = _itemInstances[index];
			Destroy(itemInstance.gameObject);
			
			_itemInstances.RemoveAt(index);
			allSessions.RemoveAt(index);
		}
		
		ExitMultiSelectMode();
	}
	
	public void MultiSelect_OpenLabels()
	{
		_multiselectLabelSelect.SetActive(true);
		_labelAsignDropdown.ClearOptions();
		
		var labels = new List<string>();
		this.labels.ForEach(label => labels.Add(label.name));
		
		_labelAsignDropdown.AddOptions(labels);
	}
	
	public void MultiSelect_SetLabel()
	{
		int labelIndex = _labelAsignDropdown.value;
		
		var label = labels[labelIndex];
		var allSessionsIndex = new List<int>(label.allSessionsIndex);
		
		foreach(int itemIndex in _multiSelectedItems)
		{
			if(allSessionsIndex.Contains(itemIndex))
				allSessionsIndex.Remove(itemIndex);
			
			else
				allSessionsIndex.Add(itemIndex);
		}
		
		label.allSessionsIndex = allSessionsIndex.ToArray();
		labels[labelIndex] = label;
		
		_multiselectLabelSelect.SetActive(false);
		ExitMultiSelectMode();
	}
	
	#endregion
	
	public void LoadMainScene()
	{
		StartCoroutine(r());
		IEnumerator r()
		{
			var operation = SceneManager.LoadSceneAsync("Gameplay");
			
			_loadScreenOverlay.SetActive(true);
			yield return new WaitUntil(()=> operation.isDone);
		}
	}
	
	public static void SaveCurrentSessionData(Session session = null)
	{
		if(session != null)
			LoadedData = session;
		
		if(!SaveManager.TryLoad<UserData>(SAVE_KEY, out var userData))
			userData = new UserData();
		
		if(userData.allSessions.IsNullOrEmpty())
			userData.allSessions = new Session[0];
		
		if(userData.allSessions.IsInsideRange(_previousSessionIndex))
			userData.allSessions[_previousSessionIndex] = LoadedData;
		
		else
		{
			var allSessions = userData.allSessions.ToList();
				allSessions.Add(LoadedData);
			
			_previousSessionIndex = allSessions.Count - 1;
			userData.allSessions = allSessions.ToArray();
		}
		
		userData.previousSessionIndex = _previousSessionIndex;
		SaveManager.Save(userData, SAVE_KEY);
	}
	
	[Serializable]
	public struct LabelInfo
	{
		public string name;
		public int[] allSessionsIndex;
	}
	
	[Serializable]
	public class UserData
	{
		public Session[] allSessions;
		public int previousSessionIndex;
		public LabelInfo[] labels;
		
		public UserData(){}
		
		public UserData(Session[] allSessions, int previousSessionIndex, LabelInfo[] labels)
		{
			this.allSessions = allSessions;
			this.previousSessionIndex = previousSessionIndex;
			this.labels = labels;
		}
	}
}

[Serializable]
public class Session
{
	public string name;
	public Tab.UserData[] tabs;
	public int[] versions;
	
	public int currentTabIndex;
	
	public string dateCreated;
	public string dateModified;
}