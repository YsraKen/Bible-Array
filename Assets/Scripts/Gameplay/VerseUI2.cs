using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class VerseUI2 : BibleTextContent, IPointerClickHandler
{
	public BibleUI2 bible { get; set; }
	
	[SerializeField] private TMP_Text _mainTmp;
	[SerializeField] private TMP_Text _foreTmp;
	
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
		instance._mainTmp.text = "";
		
		if(isEmpty) return instance;
		
		if(!string.IsNullOrEmpty(verse.title))
			instance._mainTmp.text += $"<size={instance._mainTmp.fontSize * 1.15f}><b>{verse.title}</b></size>\n\n";
		
		instance._mainTmp.text += GetMainContent(verse, _mainTmp.fontSize);
		
		return instance;
	}
	
	public static string GetMainContent(Verse info, float fontSize, bool includeComments = true)
	{
		string content = $"<size={fontSize * 0.65f}><b>{info.number}</b></size> ";
		content += info.content;
		
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
		
		var comments = info.comments;
		
		if(!comments.IsNullOrEmpty())
		{
			string replace = includeComments? _mgr.VerseCommentLink: "";
			
			for(int i = 0; i < comments.Length; i++)
			{
				// string tag = $"<COMMENT[{i}]>";
				string tag = $"[[COMMENT({i})]]";
				content = content.Replace(tag, replace);
			}
		}
		
		return content;
	}
	
	public void OnPointerClick(PointerEventData data)
	{
		var position = (Vector3) data.position;
		var linkIndex = TMP_TextUtilities.FindIntersectingLink(_mainTmp, position, null);
		
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
		if(_selectBorderHighlight)
			_selectBorderHighlight?.SetActive(isHighlighted);
	}
	
	public void SetMark(string tmpFontName, string bgHex, string letterHex)
	{
		// string hex = ColorUtility.ToHtmlStringRGBA(color);
		
		string tagOpen = $"<font={quot}{tmpFontName}{quot}><mark=#{bgHex}><color=#{letterHex}>";
		string tagClose = "</color></mark></font>";
		
		if(_isMarked)
		{
			string currentBgHex = _mainTmp.text.Substring(_mainTmp.text.IndexOf("<mark=#") + 7, 8);
			string currentLetterHex = _mainTmp.text.Substring(_mainTmp.text.IndexOf("><color=#") + 9, 8);
			
			_mainTmp.text = _mainTmp.text.Remove(0, tagOpen.Length);
			_mainTmp.text = _mainTmp.text.Remove(_mainTmp.text.Length - tagClose.Length);
			
			if(currentBgHex == bgHex && currentLetterHex == letterHex)
			{
				_isMarked = false;
				
				DeleteSavedData();
				return;
			}
		}
		
		SetMark(tagOpen);
	}
	
	public void SetMark(string tagOpen, bool saveData = true)
	{
		_mainTmp.text = _mainTmp.text.Insert(0, tagOpen);
		_mainTmp.text += UserData.highlightClose;
		
		_isMarked = true;
		
		if(saveData)
			SaveData(tagOpen);
	}
	
	public void RemoveMark(string tmpFontName)
	{
		string tagOpen = $"<font={quot}{tmpFontName}{quot}><mark=#000000FF><color=#000000FF>";
		string tagClose = UserData.highlightClose;
		
		_mainTmp.text = _mainTmp.text.Remove(0, tagOpen.Length);
		_mainTmp.text = _mainTmp.text.Remove(_mainTmp.text.Length - tagClose.Length - 1, tagClose.Length);
		
		DeleteSavedData();
	}
	
	#region DataManagent
	
	public void LoadData()
	{
		string key = GetSaveKey();
		
		if(PlayerPrefs.HasKey(key))
		{
			var userData = SaveManager.Load<UserData>(key);
			SetMark(userData.highlightOpen, false);
		}
	}
	
	void SaveData(string highlightOpen)
	{
		var userData = new UserData(){ highlightOpen = highlightOpen };
		string key = GetSaveKey();
		
		SaveManager.Save(userData, key);
	}
	
	void DeleteSavedData()
	{
		string key = GetSaveKey();		
		PlayerPrefs.DeleteKey(key);
	}
	
	string GetSaveKey()
	{
		var version = bible.version;
		var book = version.Books[GameManager.Instance.CurrentBookIndex];
		int chapterIndex = GameManager.Instance.CurrentChapterIndex;
		
		string value = $"VerseData-{bible.version.NameCode}-{book.nickname}{chapterIndex}:{Index}";
		
		return value;
	}
	
	[System.Serializable]
	public class UserData
	{
		public string highlightOpen;
		
		public const string highlightClose = "</color></mark></font>";
	}
	#endregion
}