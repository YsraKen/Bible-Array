using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class VerseUI2 : BibleTextContent, IPointerClickHandler
{
	public BibleUI2 bible { get; set; }
	
	[SerializeField] private TMP_Text _mainTmp;
	[SerializeField] private TMP_Text _foreTmp;
	
	private string _content;
	
	[SerializeField] private GameObject _selectBorderHighlight;
	[SerializeField] private char quot;
	
	public int Index{ get; private set; }
	public int dataIndex { get; set; } = -1;
	
	public bool IsMarked;
	private int _markDatabaseIndex;
	
	static GameManager _mgr => GameManager.Instance;
	
	public VerseUI2 Create(int index, Verse verse, bool isEmpty = false)
	{
		var instance = Instantiate(this, transform.parent, false);
		
		instance.Index = index;
		instance._content = "";
		
		if(isEmpty) return instance;
		
		if(!string.IsNullOrEmpty(verse.title))
			instance._content += $"<size={instance._mainTmp.fontSize * 1.15f}><b>{verse.title}</b></size>\n\n";
		
		instance._content += GetMainContent(verse, _mainTmp.fontSize);
		instance._mainTmp.text = instance._content;
		
		return instance;
	}
	
	public static string GetMainContent(Verse info, float fontSize, bool includeComments = true)
	{
		string content = $"<size={fontSize * 0.65f}><b>{info.number}</b></size> ";
		content += info.content;
		
		var mgr = _mgr;
		
		setupJesusTag("<JESUS>", $"<color=#{ColorUtility.ToHtmlStringRGBA(mgr.JesusWordColor)}>");
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
			string replace = includeComments? mgr.VerseCommentLink: "";
			
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
		
		_mgr.OnVerseSelect(this);
	}
	
	public void OnSelectionHighlight(bool isHighlighted)
	{
		if(_selectBorderHighlight)
			_selectBorderHighlight?.SetActive(isHighlighted);
	}
	
	#region Marks
	
	public void SetMark(int markDatabaseIndex, bool saveData = true)
	{
		_mainTmp.fontStyle = FontStyles.Normal;
		_foreTmp.fontStyle = FontStyles.Normal;
		
		if(IsMarked)
		{
			bool isClearing = markDatabaseIndex == _markDatabaseIndex;
			
			DeleteSavedData();
			
			if(isClearing)
			{
				_mainTmp.text = _content;
				_foreTmp.text = "";
			
				IsMarked = false;
				return;
			}
		}
		
		var value = _mgr.MarkManager.MarkInfos[markDatabaseIndex].value;
		
		setFontStyles(_mainTmp);
		setFontStyles(_foreTmp);
		
		void setFontStyles(TMP_Text tmp)
		{
			if(value.b) tmp.fontStyle |= (FontStyles)(1 << 0);
			if(value.i) tmp.fontStyle |= (FontStyles)(1 << 1);
			if(value.s) tmp.fontStyle |= (FontStyles)(1 << 6);
			if(value.u) tmp.fontStyle |= (FontStyles)(1 << 2);
		}
		
		string bgHex = value.GetBackgroundHex();
		string letterHex = value.GetLetterHex();
		
		_mainTmp.text = $"<mark=#{bgHex}>{_content}</mark>";
		_foreTmp.text = $"<color=#{letterHex}>{_content}</color>";
		
		IsMarked = true;
		_markDatabaseIndex = markDatabaseIndex;
		
		if(saveData)
			SaveData();
	}
	
	public void RemoveMark()
	{
		_mainTmp.fontStyle = FontStyles.Normal;
		_foreTmp.fontStyle = FontStyles.Normal;
		
		_mainTmp.text = _content;
		_foreTmp.text = "";
	
		IsMarked = false;
		
		DeleteSavedData();
	}
	
	#endregion
	
	#region DataManagent
	
	void SaveData()
	{
		var mgr = _mgr;
		
		if(dataIndex < 0)
		{
			dataIndex = mgr.ChapterUserData.verseDatas.Count;
			
			var data = new ChapterUserData.VerseData()
			{
				index = Index,
				versionIndex = bible.version.GetIndex(),
				markIndex = _markDatabaseIndex
			};
			
			mgr.ChapterUserData.verseDatas.Add(data);
		}
		else
		{
			var data = mgr.ChapterUserData[dataIndex];
				data.markIndex = _markDatabaseIndex;
			
			mgr.ChapterUserData[dataIndex] = data;
		}
	}
	
	void DeleteSavedData()
	{
		var mgr = _mgr;
		
		mgr.MarkManager.RemoveOn
		(
			_markDatabaseIndex,
			bible.version.Language.GetIndex(),
			bible.version.GetIndex(),
			mgr.CurrentBookIndex,
			mgr.CurrentChapterIndex,
			Index
		);
		
		if(mgr.ChapterUserData.verseDatas.IsInsideRange(dataIndex))
			mgr.ChapterUserData.verseDatas.RemoveAt(dataIndex);
		
		_markDatabaseIndex = -1;
		dataIndex = -1;
	}
	
	#endregion
}