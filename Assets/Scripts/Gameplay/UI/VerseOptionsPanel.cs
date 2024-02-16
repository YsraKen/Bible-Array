using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class VerseOptionsPanel : MonoBehaviour
{
	public string currentTmpFontName = "LiberationSans SDF - Fallback";
	
	public HighlightInfo[] highlights;
	public Dropdown _highlightGrpSelect;
	public HighlightPanel _highlightPanel;
	public Sprite _popupIcon;
	
	[Space]
	public Button compareButton;
	public VerseCompare comparePanel;
	public GameObject copyPopup;
	
	void Start()
	{
		GameManager.Instance.onVerseSelectUpdate += OnVerseSelectUpdate;
	}
	
	void OnEnable()
	{
		_highlightGrpSelect.ClearOptions();
		var options = new List<Dropdown.OptionData>();
		
		foreach(var info in _highlightPanel.Infos)
			options.Add(new Dropdown.OptionData(info.name));
		
		options.Add(new Dropdown.OptionData("Edit...", _popupIcon));
		_highlightGrpSelect.AddOptions(options);
	}
	
	public void OnHighlightButton(Transform transform)
	{
		int index = transform.GetSiblingIndex();
		var info = highlights[index];
		
		foreach(var selected in GameManager.Instance.SelectedVerses)
			// selected.SetMark(currentTmpFontName, info.GetBackgroundHex(), info.GetLetterHex());
			selected.SetMark(info.GetBackgroundHex(), info.GetLetterHex());
		
		OnClose();
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
	
	public void OnHighlightGroupSelect(int value)
	{
		if(value == (_highlightGrpSelect.options.Count - 1))
		{
			_highlightPanel.gameObject.SetActive(true);
			return;
		}
	}
	
	[System.Serializable]
	public struct HighlightInfo
	{
		public string name;
		public Color background;
		public Color letter;
		
		public string GetBackgroundHex() => ColorUtility.ToHtmlStringRGBA(background);
		public string GetLetterHex() => ColorUtility.ToHtmlStringRGBA(letter);
	}
}