using UnityEngine;

public class VerseOptionsPanel : MonoBehaviour
{
	public string currentTmpFontName = "LiberationSans SDF - Fallback";
	public HighlightInfo[] highlights;
	
	public void OnHighlightButton(Transform transform)
	{
		int index = transform.GetSiblingIndex();
		var info = highlights[index];
		
		foreach(var selected in GameManager.Instance.SelectedVerses)
			selected.SetMark(currentTmpFontName, info.GetBackgroundHex(), info.GetLetterHex());
	}
	
	public void OnCompareButton()
	{
		
	}
	
	public void OnCopyButton()
	{
		
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
	
	[System.Serializable]
	public struct HighlightInfo
	{
		public Color background;
		public Color letter;
		
		public string GetBackgroundHex() => ColorUtility.ToHtmlStringRGBA(background);
		public string GetLetterHex() => ColorUtility.ToHtmlStringRGBA(letter);
	}
}