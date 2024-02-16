using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
		panel.versionSelect.StartSelection(onConfirm, onCancel);
		
		void onConfirm(Version newVersion)
		{
			var items = new List<VerseCompareItem>(panel.items);
				items.Insert(0, panel.defaultItem);
			
			if(items.TryFindIndex(item => item.version == newVersion, out int existIndex))
			{
				var scrollPosition = Mathf.InverseLerp(items.Count - 1, 0, existIndex);
				var item = items[existIndex];
				
				panel.scroll.SetPosition(Vector2.up * scrollPosition, 0.5f, ()=> item.highlighter.SetActive(true));
				
				return;
			}
			
			version = newVersion;
			UpdateContent();
			
			panel.scroll.SetPosition(Vector2.zero, 0.3f, ()=> highlighter.SetActive(true));
		}
		
		void onCancel()
		{
			var items = new List<VerseCompareItem>(panel.items);
				items.Insert(0, panel.defaultItem);
			
			if(items.Exists(item => item != this && item.version == version))
				panel.DeleteItem(this);
		}
	}
	
	public void OnDelete() => panel.DeleteItem(this);
}