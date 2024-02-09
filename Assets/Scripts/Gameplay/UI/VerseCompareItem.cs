using UnityEngine;
using UnityEngine.UI;

public class VerseCompareItem : MonoBehaviour
{
	public Text versionTxt;
	public Text txt;
	
	public Version version;
	public GameObject highlighter;
	
	public string text
	{
		get => txt.text;
		set => txt.text = value;
	}
	
	VerseCompare _panel;
	VerseCompare panel
	{
		get
		{
			if(!_panel)
				_panel = GetComponentInParent<VerseCompare>(true);
			
			return _panel;
		}
	}
	
	public void UpdateContent()
	{
		versionTxt.text = version.Name;
		
		var mgr = GameManager.Instance;
		
		int bookIndex = mgr.CurrentBookIndex;
		int chapterIndex = mgr.CurrentChapterIndex;
		
		text = "";
		
		foreach(var selected in panel.selectedVerses)
		{
			var info = version.Books[bookIndex][chapterIndex][selected.Index];
			text += VerseUI2.GetMainContent(info, txt.fontSize, false) + "\n";
		}
	}
	
	public void OnVersionSelect()
	{
		panel.versionSelect.StartSelection(onSelect);
		
		void onSelect(Version newVersion)
		{
			bool isVersionFromDefault = panel.defaultItem.version == newVersion;
			bool isVersionFromItems = panel.items.TryFindIndex(item => item.version == newVersion, out int existIndex);
			
			if(isVersionFromDefault || isVersionFromItems)
			{
				if(existIndex != -1)
				{
					var scrollPosition = isVersionFromDefault? 1: Mathf.InverseLerp(panel.items.Count - 1, 0, existIndex);
					var item = isVersionFromDefault?  panel.defaultItem: panel.items[existIndex];
					
					panel.scroll.SetPosition(Vector2.up * scrollPosition, 0.5f, ()=> item.highlighter.SetActive(true));
				}
				
				return;
			}
			
			version = newVersion;
			UpdateContent();
			
			highlighter.SetActive(true);
		}
	}
	
	public void OnDelete() => panel.DeleteItem(this);
}