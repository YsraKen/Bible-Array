using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class VerseUI : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private TMP_Text _tmp;
	
	private static char[] _verseNumberChars = { '⁰', '¹', '²', '³', '⁴', '⁵', '⁶', '⁷', '⁸', '⁹' };
	
	public string ToVerseNumber(int n)
	{
		string n_Str = n.ToString();
		string output = string.Empty;
		
		foreach(char c in n_Str)
		{
			int index = int.Parse(c.ToString());
			output += _verseNumberChars[index];
		}
		
		return output;
	}
	
	public int test;
	public string result;
	
	private Verse.Comment[] _comments;
	
	[ContextMenu("Test")] void Test()
	{
		test = Random.Range(0, 10000);
		result = ToVerseNumber(test);
	}
	
	public void Setup(Verse verse, int index, string commentLink)
	{
		_tmp.text = "";
		
		if(!string.IsNullOrEmpty(verse.title))
			_tmp.text += $"<size={_tmp.fontSize * 1.25f}><b>{verse.title}</b></size>\n\n";
		
		// _tmp.text += $"{ToVerseNumber(index + 1)} ";
		_tmp.text += $"<size={_tmp.fontSize * 0.65f}><b>{index + 1}</b></size> ";
		
		string content = verse.content;
		
		/* string commentSearchTag = "[COMMENT[";
		int commentSearchIndex = 0;
		int commentCount = 0;
		
		while(commentSearchIndex < content.Length)
		{
			commentSearchIndex = content.IndexOf(commentSearchTag, commentSearchIndex);
			if(commentSearchIndex < 0) break;
			
			commentCount ++;
			commentSearchIndex += commentSearchTag.Length;
		} */
		
		// for(int i = 0; i < commentCount; i++)
		
		_comments = verse.comments;
		
		for(int i = 0; i < _comments.Length; i++)
		{
			string tag = $"[COMMENT[{i}]]";
			content = content.Replace(tag, commentLink);
		}
		
		_tmp.text += content;
	}
	
	public void OnPointerClick(PointerEventData data)
	{
		var position = (Vector3) data.position;
		var linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmp, position, null);
		
		if(linkIndex > -1)
		{
			// _comments[linkIndex].DebugLog(out string msg);
			
			var comment = _comments[linkIndex];
			string title = comment.number;
			string msg = "";
			
			foreach(var content in comment.contents)
				msg += $"{content.ft} <i><b>{content.body}</b></i>";
			
			BibleUI.ShowPopup(title, msg, position);
			
			return;
		}
		
		// highlight
		// copy
		// report
	}
}