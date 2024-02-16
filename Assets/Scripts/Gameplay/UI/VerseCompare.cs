using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class VerseCompare : MonoBehaviour
{
	public VerseCompareItem defaultItem;
	
	public List<VerseCompareItem> items = new List<VerseCompareItem>();
	public List<VerseUI2> selectedVerses = new List<VerseUI2>();
	
	public Transform addItemButton;
	public ScrollRect scroll;
	
	public VersionSelectPanel versionSelect;
	public Sprite recentIcon;
	public Sprite favoriteIcon;
	
	public GameObject copyPopup;
	
	void Awake()
	{
		if(SaveManager.TryLoad<UserData>("VerseCompare", out var userData))
		{
			defaultItem.version = GameManager.Instance.Versions[userData.defaultVersionIndex];
			
			for(int i = 0; i < items.Count; i++)
				Destroy(items[i].gameObject);
			
			items.Clear();
			
			var itemsParent = defaultItem.transform.parent;
			
			for(int i = 0; i < userData.Length; i++)
			{
				var version = GameManager.Instance.Versions[userData[i]];
				
				var item = Instantiate(defaultItem, itemsParent, false);
					item.version = version;
					item.UpdateContent();
				
				items.Add(item);
			}
			
			addItemButton.SetAsLastSibling();
		}
	}
	
	void OnDisable()
	{
		int count = items.Count;
		var versionIndexes = new int[count];
		
		var mgr = GameManager.Instance;
		
		for(int i = 0; i < count; i++)
			versionIndexes[i] = Array.FindIndex(mgr.Versions, version => version == items[i].version);
		
		var userData = new UserData()
		{
			defaultVersionIndex = Array.FindIndex(mgr.Versions, version => version == defaultItem.version),
			versionIndexes = versionIndexes
		};
		
		SaveManager.Save(userData, "VerseCompare");
	}
	
	public void Show(List<VerseUI2> selections)
	{
		selectedVerses = selections;
		
		defaultItem.version = selectedVerses[0].bible.version;
		defaultItem.UpdateContent();
		defaultItem.highlighter.SetActive(true);
		
		items.ForEach(item => item.UpdateContent());
		
		gameObject.SetActive(true);
		scroll.verticalNormalizedPosition = 1f;
	}
	
	public void AddItem()
	{
		var item = Instantiate(defaultItem, defaultItem.transform.parent, false);
			items.Add(item);
			item.UpdateContent();
			item.OnVersionSelect();
		
		addItemButton.SetAsLastSibling();
	}
	
	public void DeleteItem(VerseCompareItem item)
	{
		if(item == defaultItem)
			return;
		
		items.Remove(item);
		Destroy(item.gameObject);
	}
	
	public void CopyAll()
	{
		var mgr = GameManager.Instance;
		int bookIndex = mgr.CurrentBookIndex;
		int chapterIndex = mgr.CurrentChapterIndex;
		
		string text =$"{mgr.GeneralInfo.bookChapterVerseInfos[bookIndex].name} {chapterIndex + 1}\n";
		
		foreach(var item in items)
			text += $"\n[{item.version.NameCode}]\n{item.text}";
		
		string pattern = @"<[^>]+>";
        text = Regex.Replace(text, pattern, "");
		
		GUIUtility.systemCopyBuffer = text;
		Debug.Log(text, this);
		
		copyPopup.SetActive(true);
	}
	
	[System.Serializable]
	public struct UserData
	{
		public int defaultVersionIndex;
		public int[] versionIndexes;
		
		public int Length => versionIndexes.Length();
		
		public int this[int index]
		{
			get => versionIndexes[index];
			set => versionIndexes[index] = value;
		}
	}
}