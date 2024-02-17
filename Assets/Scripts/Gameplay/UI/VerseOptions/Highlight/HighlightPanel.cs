using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightPanel : MonoBehaviour
{
	public int currentSelectedIndex { get; set; }
	
	[SerializeField] private HighlightPanelItem _itemTemplate;
	[SerializeField] private ColorPanel _colorPanel;
	
	[field: SerializeField]
	public List<HighlightInfo> Infos { get; private set; } = new List<HighlightInfo>();
	
	[SerializeField, EnumData(typeof(ScreenOrientation))]
	private RectTransform[] _screenOrientationRefs;
	
	[SerializeField] private RectTransform _panel;
	[SerializeField] private ScrollRect _scroll;
	
	List<HighlightPanelItem> _itemInstances = new List<HighlightPanelItem>();
	
	public static HighlightPanel Instance { get; private set; }
	
	void Awake() => Instance = this;
	
	void OnEnable()
	{
		UpdatePanelOrientation(GameManager.Instance.ScreenOrientation);
		
		for(int i = 0; i < _itemInstances.Count; i++)
			Destroy(_itemInstances[i].gameObject);
		
		_itemInstances.Clear();
		_itemTemplate.gameObject.SetActive(true);
		
		var itemTemplateT = _itemTemplate.transform;
		var templateParent = itemTemplateT.parent;
		
		int index = 0;
		
		foreach(var info in Infos)
		{
			var item = Instantiate(_itemTemplate, templateParent, false);
				item.Init(index ++, info);
			
			_itemInstances.Add(item);
		}
		
		UpdateSelectedItem(currentSelectedIndex);
		
		itemTemplateT.SetAsLastSibling();
		_itemTemplate.gameObject.SetActive(false);
		
		var scrollPosition = new Vector2(0f, Mathf.InverseLerp(_itemInstances.Count - 1, 0, currentSelectedIndex));
		_scroll.SetPosition(scrollPosition, 0.5f, ()=> _itemInstances[currentSelectedIndex].Ping());
	}
	
	void OnDisable()
	{
		VerseOptionsPanel.Instance.UpdateHighlightGroupsInfo();
		SaveData();
	}
	
	public HighlightInfo GetActiveInfo() => Infos[currentSelectedIndex];
	
	private void UpdateSelectedItem(int index)
	{
		for(int i = 0; i < _itemInstances.Count; i++)
		{
			var item = _itemInstances[i];
			bool isSelected = i == index;
			
			item.SetSelected(isSelected);
			item.SetFoldout(isSelected);
		}
		
		currentSelectedIndex = index;
	}
	
	public void UpdatePanelOrientation(ScreenOrientation orientation)
	{
		var screenRef = _screenOrientationRefs[(int) orientation];
		
		_panel.position = screenRef.position;
		_panel.sizeDelta = screenRef.sizeDelta;
	}
	
	public void OnCreate()
	{
		var info = new HighlightInfo();
		var templateT = _itemTemplate.transform;
		
		var item = Instantiate(_itemTemplate, templateT.parent, false);
			item.gameObject.SetActive(true);
			item.Init(_itemInstances.Count, info);
		
		Infos.Add(info);
		_itemInstances.Add(item);
		
		templateT.SetAsLastSibling();
		UpdateSelectedItem(_itemInstances.Count - 1);
		
		_scroll.SetPosition(Vector2.zero, 0.5f, ()=> item.Ping());
	}
	
	public void OnItemEdit(string newName, int index) => Infos[index].name = newName;
	
	public void OnItemDelete(HighlightPanelItem item)
	{
		int index = _itemInstances.IndexOf(item);
		
		Infos.RemoveAt(index);
		_itemInstances.RemoveAt(index);
		
		UpdateSelectedItem(Mathf.Clamp(currentSelectedIndex, 0, _itemInstances.Count - 1));
	}
	
	public void OpenColorPanel(int infoIndex, int markIndex, Action<HighlightInfo.Mark> onApply)
	{
		var mark = Infos[infoIndex].marks[markIndex];
		_colorPanel.Open(mark, onSubmit);
		
		void onSubmit(HighlightInfo.Mark newMark)
		{
			Infos[infoIndex].marks[markIndex] = newMark;
			onApply(newMark);
		}
	}
	
	public void LoadData()
	{
		if(SaveManager.TryLoad<UserData>("Highlights", out var userData))
		{
			currentSelectedIndex = userData.currentSelectedIndex;
			
			Infos.Clear();
			Array.ForEach(userData.infos, info => Infos.Add(info));
		}
	}
	
	public void SaveData()
	{
		var userData = new UserData()
		{
			currentSelectedIndex = currentSelectedIndex,
			infos = Infos.ToArray()
		};
		
		SaveManager.Save<UserData>(userData, "Highlights");
	}
	
	[Serializable]
	public class UserData
	{
		public int currentSelectedIndex;
		public HighlightInfo[] infos;
	}
}

[Serializable]
public class HighlightInfo
{
	public string name;
	public Mark[] marks;
	
	public HighlightInfo(string name = null)
	{
		this.name = name;
		marks = new Mark[1]{ Mark.Default };
	}
	
	[Serializable]
	public struct Mark
	{
		public string name;
		public Color background;
		public Color letter;
		
		public bool b;
		public bool i;
		public bool s;
		public bool u;
		
		public static Mark Default => new Mark()
		{
			background = Color.yellow,
			letter = Color.black
		};
		
		public string GetBackgroundHex() => ColorUtility.ToHtmlStringRGBA(background);
		public string GetLetterHex() => ColorUtility.ToHtmlStringRGBA(letter);
	}
}