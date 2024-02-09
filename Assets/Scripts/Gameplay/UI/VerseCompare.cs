using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
	
	public VerseOptionsPanel mainPanel;
	
	public void Show(List<VerseUI2> selections)
	{
		selectedVerses = selections;
		
		defaultItem.version = selectedVerses[0].bible.version;
		defaultItem.UpdateContent();
		defaultItem.highlighter.SetActive(true);
		
		items.ForEach(item => item.UpdateContent());
		
		gameObject.SetActive(true);
	}
	
	public void AddItem()
	{
		var template = items.Count > 0? items.GetLastElement(): defaultItem;
		var newItem = Instantiate(template, template.transform.parent, false);
		
		items.Add(newItem);
		addItemButton.SetAsLastSibling();
		
		scroll.SetPosition(Vector2.down, 0.5f);
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
		mainPanel.CopyElements(selectedVerses);
	}
}