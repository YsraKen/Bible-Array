using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SessionItem : MonoBehaviour
{
	[SerializeField] private TMP_Text _nameTmp;
	[SerializeField] private Text _tabsTxt;
	[SerializeField] private Text _versionsTxt;
	[SerializeField] private Text _dateTxt;
	[SerializeField] private GameObject _glow;
	
	[field: SerializeField]
	public Toggle MultiSelectToggle { get; private set; }
	
	[Space]
	[SerializeField] private GeneralInformation _genInfo;
	
	public int Index { get; private set; }
	
	private Coroutine _longPressRoutine;
	
	public void Init(int index, Session info)
	{
		Index = index;
		
		UpdateName(info);
		
		#region Tabs
		
		if(info.tabs.IsNullOrEmpty())
			_tabsTxt.text = "[No active tabs open]";
		
		else
		{
			_tabsTxt.text = "";
			
			foreach(var tab in info.tabs)
				_tabsTxt.text += $"[{_genInfo.bookChapterVerseInfos[tab.bookIndex].name} {tab.chapterIndex + 1}] ";
			
			// remove excess space that is being generated during the loop
			_tabsTxt.text = _tabsTxt.text.Substring(0, _tabsTxt.text.Length - 1);
		}
		
		#endregion
		
		#region Versions
		
		if(info.versions.IsNullOrEmpty())
			_versionsTxt.text = "No bible verions were open";
		
		else
		{
			_versionsTxt.text = "";
			
			foreach(int versionIndex in info.versions)
				_versionsTxt.text += $"{_genInfo.allVersions[versionIndex].NameCode}, ";
			
			// remove excess chars that is being generated during the loop
			_versionsTxt.text = _versionsTxt.text.Substring(0, _versionsTxt.text.Length - 2);
		}
		
		#endregion
		
		#region Dates
		
		string created = $"<color=green>Created: {info.dateCreated}</color>";
		string modified = $"<color=yellow>Updated: {info.dateModified}</color>";
		
		_dateTxt.text = $"{created}\n{modified}";
		
		#endregion
	}
	
	public void OnSelect() => SessionSelect.Instance.OnSessionSelect(this);
	public void OnEdit() => SessionSelect.Instance.EditSession(this);
	
	public void UpdateName(Session info)
	{
		_nameTmp.text = "";
		_nameTmp.text = info.name;
		
		if(string.IsNullOrEmpty(_nameTmp.text))
			_nameTmp.text = info.dateCreated;
		
		if(string.IsNullOrEmpty(_nameTmp.text))
			_nameTmp.text = info.dateModified;
		
		if(string.IsNullOrEmpty(_nameTmp.text))
			_nameTmp.text = "<i>Untitled Session</i>";
	}
	
	public void OnPointerDown()
	{
		_longPressRoutine = StartCoroutine(r());
		IEnumerator r()
		{
			var mgr = SessionSelect.Instance;
			yield return new WaitForSeconds(mgr.MultiSelectHoldTrigger);
			
			MultiSelectToggle.isOn = true;
			mgr.EnterMultiSelectMode();
		}
	}
	
	public void OnPointerUp()
	{
		if(_longPressRoutine != null)
			StopCoroutine(_longPressRoutine);
	}
	
	public void OnMultiSelect() => SessionSelect.Instance.OnMultiselectAddItem(MultiSelectToggle);
	
	public void Ping() => _glow.SetActive(true);
}