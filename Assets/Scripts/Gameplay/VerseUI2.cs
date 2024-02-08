using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class VerseUI2 : MonoBehaviour, IPointerClickHandler
{
	public BibleUI2 bible { get; set; }
	
	[SerializeField] private TMP_Text _tmp;
	[SerializeField] private GameObject _selectBorderHighlight;
	[SerializeField] private char quot;
	
	// private Verse.Comment[] _comments;
	public int Index{ get; private set; }
	private bool _isMarked;
	
	static GameManager _mgr => GameManager.Instance;
	
	public VerseUI2 Create(int index, Verse verse, bool isEmpty = false)
	{
		var instance = Instantiate(this, transform.parent, false);
		
		instance.Index = index;
		instance._tmp.text = "";
		
		if(isEmpty) return instance;
		
		/* if(index == 0)
		{
			var book = bible.version.Books[_mgr.CurrentBookIndex];
			string bookName = book.fancyName;
			
			if(string.IsNullOrEmpty(bookName))
				bookName = book.Name;
			
			instance._tmp.text += $"<size={instance._tmp.fontSize * 1.5f}><b>{_mgr.CurrentChapterIndex + 1}\n{bookName}</b></size>\n\n";
		} */
		
		if(!string.IsNullOrEmpty(verse.title))
			instance._tmp.text += $"<size={instance._tmp.fontSize * 1.15f}><b>{verse.title}</b></size>\n";
		
		instance._tmp.text += $"<size={instance._tmp.fontSize * 0.65f}><b>{verse.number}</b></size> ";
		
		// if(isMarked)
		// {
			// _isMarked = isMarked;
		// }
		
		string content = verse.content;
		
		setupJesusTag("<JESUS>", $"<color=#{ColorUtility.ToHtmlStringRGBA(_mgr.JesusWordColor)}>");
		setupJesusTag("</JESUS>", "</color>");
		
		void setupJesusTag(string tag, string insert)
		{
			for(int i = 0; i < content.Length; i++)
			{
				int index = content.IndexOf(tag);
				if(index < 0) continue;
				
				content = content.Remove(index, tag.Length).Insert(index, insert);
				
				i += insert.Length;
				i --;
			}
		}
		
		var comments = verse.comments;
		string commentLink = _mgr.VerseCommentLink;
		
		if(!comments.IsNullOrEmpty())
		{
			for(int i = 0; i < comments.Length; i++)
			{
				// string tag = $"<COMMENT[{i}]>";
				string tag = $"[[COMMENT({i})]]";
				content = content.Replace(tag, commentLink);
			}
		}
		
		instance._tmp.text += content;
		
		return instance;
	}
	
	public void OnPointerClick(PointerEventData data)
	{
		var position = (Vector3) data.position;
		var linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmp, position, null);
		
		if(linkIndex > -1)
		{
			_mgr.ShowVerseCommentPopup(bible.version, Index, linkIndex, position);
			return;
		}
		
		// highlight
		// copy
		// report
		
		_mgr.OnVerseSelect(this/* , bible.version */);
	}
	
	public void OnSelectionHighlight(bool isHighlighted)
	{
		_selectBorderHighlight.SetActive(isHighlighted);
	}
	
	public void SetMark(string tmpFontName, string bgHex, string letterHex)
	{
		// string hex = ColorUtility.ToHtmlStringRGBA(color);
		
		string tagOpen = $"<font={quot}{tmpFontName}{quot}><mark=#{bgHex}><color=#{letterHex}>";
		string tagClose = "</color></mark></font>";
		
		if(_isMarked)
		{
			string currentBgHex = _tmp.text.Substring(_tmp.text.IndexOf("<mark=#") + 7, 8);
			string currentLetterHex = _tmp.text.Substring(_tmp.text.IndexOf("><color=#") + 9, 8);
			
			_tmp.text = _tmp.text.Remove(0, tagOpen.Length);
			_tmp.text = _tmp.text.Remove(_tmp.text.Length - tagClose.Length);
			
			if(currentBgHex == bgHex && currentLetterHex == letterHex)
			{
				_isMarked = false;
				return;
			}
		}
		
		_tmp.text = _tmp.text.Insert(0, tagOpen);
		_tmp.text += tagClose;
		
		_isMarked = true;
	}
	
	public void RemoveMark(string tmpFontName)
	{
		string tagOpen = $"<font={quot}{tmpFontName}{quot}><mark=#000000FF><color=#000000FF>";
		string tagClose = "</color></mark></font>";
		
		_tmp.text = _tmp.text.Remove(0, tagOpen.Length);
		_tmp.text = _tmp.text.Remove(_tmp.text.Length - tagClose.Length - 1, tagClose.Length);
	}
}