using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class VerseOptionsPanel : MonoBehaviour
{
	public Transform[] tabs;
	public GameObject[] tabBodies;
	
	public Dropdown _highlightGrpSelect;
	public GameObject _highlightQuickButtonTemplate;
	public HorizontalLayoutGroup _highlightQuickButtonsLayoutGroup;
	
	[Space]
	public GameObject _deleteHighlightButton;
	public RectTransform _highlightQuickBtnsScrollViewport;
	public float _highlightQuickBtnsScrollViewportOffset = 65f;
	
	List<GameObject> _highlightQuickButtons = new List<GameObject>();
	
	public HighlightPanel _highlightPanel;
	public Sprite _popupIcon;
	
	[Space]
	public Button compareButton;
	public VerseCompare comparePanel;
	public GameObject copyPopup;
	
	int _highlightGrpSelect_PreviousValue;
	
	[SerializeField] private BibleLayout _bblLayout;
	
	public static VerseOptionsPanel Instance { get; private set; }
	
	void Awake()
	{
		Instance = this;
		_highlightPanel.LoadData();
	}
	
	void Start()
	{
		GameManager.Instance.onVerseSelectUpdate += OnVerseSelectUpdate;
	}
	
	void OnEnable()
	{
		RefreshHighlightGrpDropdownOptions();
		RefreshHighlightQuickButtons();
		
		CheckRemoveHighlightOption();
	}
	
	public void OnTabSelect(Transform tab)
	{
		int index = tab.GetSiblingIndex();
		
		for(int i = 0; i < tabs.Length; i++)
		{
			bool isActive = i == index;
			
			var highlighter = tabs[i].GetChild(0).gameObject;
				highlighter.SetActive(isActive);
			
			tabBodies[i].SetActive(isActive);
		}
	}
	
	public void OnHighlightButton(Transform transform)
	{
		int index = transform.GetSiblingIndex();
		
		var info = _highlightPanel.Infos[_highlightPanel.currentSelectedIndex];
		var mark = info.marks[index];
		
		var gameMgr = GameManager.Instance;
		var markMgr = gameMgr.MarkManager;
		
		foreach(var selected in gameMgr.SelectedVerses)
		{
			int markDatabaseIndex = markMgr.GetDatabaseIndex(info.name, mark, selected.bible.version, selected.Index);
			selected.SetMark(markDatabaseIndex);
		}
		
		OnClose();
		_bblLayout.Repaint();
	}
	
	public void OnRemoveHighlightButton()
	{
		foreach(var selected in GameManager.Instance.SelectedVerses)
			selected.RemoveMark();
		
		OnClose();
		_bblLayout.Repaint();
	}
	
	public void OnCompareButton()
	{
		var selections = SortSelections();
		comparePanel.Show(selections);
	}
	
	public void OnCopyButton()
	{
		var selections = SortSelections();
		CopyElements(selections);
	}
	
	public void OnAddNotesButton()
	{
		
	}
	
	public void OnReportButton()
	{
		
	}
	
	public void OnClose()
	{
		var mgr = GameManager.Instance;
		
		for(int i = mgr.SelectedVerses.Count - 1; i >= 0; i--)
			mgr.OnVerseSelect(mgr.SelectedVerses[i]);
	}
	
	private void OnVerseSelectUpdate()
	{
		Version previousVersion = null;
		bool isComparable = true;
		
		foreach(var selected in GameManager.Instance.SelectedVerses)
		{
			var currentVersion = selected.bible.version;
			
			if(previousVersion && currentVersion != previousVersion)
			{
				isComparable = false;
				break;
			}
			
			previousVersion = currentVersion;
		}
		
		compareButton.interactable = isComparable;
	}
	
	private List<VerseUI2> SortSelections()
	{
		return GameManager.Instance.SelectedVerses.OrderBy(selected => selected.Index).ToList();
	}
	
	public void CopyElements(List<VerseUI2> selections)
	{
		string text = "";
		
		var mgr = GameManager.Instance;
		int bookIndex = mgr.CurrentBookIndex;
		int chapterIndex = mgr.CurrentChapterIndex;
		
		bool hasMultipleVersions = false;
		var previousVersion =  selections[0].bible.version;
		
		foreach(var selected in selections)
		{
			hasMultipleVersions = previousVersion != selected.bible.version;
			if(hasMultipleVersions) break;
		}
		
		string bookName = hasMultipleVersions?
			$"{GameManager.Instance.GeneralInfo.bookChapterVerseInfos[bookIndex].name} {chapterIndex + 1}":
			$"{previousVersion.Books[bookIndex].Name} {chapterIndex + 1} {previousVersion.NameCode}";
		
		text += bookName;
		text += "\n";
		
		previousVersion = null;
		
		foreach(var selected in selections)
		{
			var info = selected.bible.version.Books[bookIndex][chapterIndex][selected.Index];
			string content = VerseUI2.GetMainContent(info, 1f, false);
			
			content = content.Insert(0, "[");
			content = content.Insert(content.IndexOf(' '), "]");
			
			if(hasMultipleVersions)
			{
				var currentVersion = selected.bible.version;
				
				if(currentVersion != previousVersion)
					content = content.Insert(0, $"\n{currentVersion.NameCode}\n");
				
				previousVersion = currentVersion;
			}
			
			text += content + "\n";
		}
		
        string pattern = @"<[^>]+>";
        text = Regex.Replace(text, pattern, "");
		
		GUIUtility.systemCopyBuffer = text;
		Debug.Log(text, this);
		
		copyPopup.SetActive(true);
	}
	
	#region Highlight Groups
	
	public void OnHighlightGroupSelect(int value)
	{
		_highlightGrpSelect_PreviousValue = _highlightPanel.currentSelectedIndex;
		_highlightPanel.currentSelectedIndex = value;
		
		bool isEditModeSelected = value == (_highlightGrpSelect.options.Count - 1);
		
		if(isEditModeSelected)
		{
			_highlightPanel.currentSelectedIndex = _highlightGrpSelect_PreviousValue;
			_highlightPanel.gameObject.SetActive(true);
			
			_highlightGrpSelect.SetValueWithoutNotify(_highlightGrpSelect_PreviousValue);
			
			return;
		}
		
		RefreshHighlightQuickButtons();
	}
	
	public void UpdateHighlightGroupsInfo()
	{
		RefreshHighlightGrpDropdownOptions();
		RefreshHighlightQuickButtons();
		
		_highlightGrpSelect.SetValueWithoutNotify(_highlightPanel.currentSelectedIndex);
	}
	
	private void RefreshHighlightGrpDropdownOptions()
	{
		_highlightGrpSelect.ClearOptions();
		var options = new List<Dropdown.OptionData>();
		
		foreach(var info in _highlightPanel.Infos)
		{
			string name = string.IsNullOrEmpty(info.name)? "(untitled)": info.name;
			options.Add(new Dropdown.OptionData(name));
		}
		
		options.Add(new Dropdown.OptionData("Edit...", _popupIcon));
		
		_highlightGrpSelect.AddOptions(options);
		_highlightGrpSelect.SetValueWithoutNotify(_highlightPanel.currentSelectedIndex);
	}
	
	private void RefreshHighlightQuickButtons()
	{
		for(int i = 0; i < _highlightQuickButtons.Count; i++)
			Destroy(_highlightQuickButtons[i]);
		
		_highlightQuickButtons.Clear();
		
		var info = _highlightPanel.Infos[_highlightPanel.currentSelectedIndex];
		
		_highlightQuickButtonTemplate.SetActive(true);
		
		var templateT = _highlightQuickButtonTemplate.transform;
		var parent = templateT.parent;
		
		foreach(var mark in  info.marks)
		{
			var button = Instantiate(_highlightQuickButtonTemplate, parent, false);
			HighlightPanelItem.UpdateElementValues(button, mark, "A");
			
			_highlightQuickButtons.Add(button);
		}
		
		templateT.SetAsLastSibling();
		
		_highlightQuickButtonTemplate.SetActive(false);
		_highlightQuickButtonsLayoutGroup.Poke();
	}
	
	public void CheckRemoveHighlightOption()
	{
		bool hasHighlightSelection = GameManager.Instance.SelectedVerses.Exists(selected => selected.IsMarked);
		_deleteHighlightButton.SetActive(hasHighlightSelection);
		
		var sizeDelta = _highlightQuickBtnsScrollViewport.sizeDelta;
			sizeDelta.x = hasHighlightSelection? -_highlightQuickBtnsScrollViewportOffset: 0f;
			
		var anchoredPosition = _highlightQuickBtnsScrollViewport.anchoredPosition;
			anchoredPosition.x = hasHighlightSelection? _highlightQuickBtnsScrollViewportOffset / 2f: 0f;
		
		_highlightQuickBtnsScrollViewport.sizeDelta = sizeDelta;
		_highlightQuickBtnsScrollViewport.anchoredPosition = anchoredPosition;
	}
	
	#endregion
}