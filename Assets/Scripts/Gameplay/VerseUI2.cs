using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class VerseUI2 : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private TMP_Text _tmp;
	
	private Verse.Comment[] _comments;
	
	public VerseUI2 Create(Verse verse, int index)
	{
		var instance = Instantiate(this, transform.parent, false);
		
		instance._tmp.text = "";
		
		if(!string.IsNullOrEmpty(verse.title))
			instance._tmp.text += $"<size={instance._tmp.fontSize * 1.25f}><b>{verse.title}</b></size>\n\n";
		
		instance._tmp.text += $"<size={instance._tmp.fontSize * 0.65f}><b>{index + 1}</b></size> ";
		
		string content = verse.content;
		
		instance._comments = verse.comments;
		string commentLink = GameManager.Instance.VerseCommentLink;
		
		if(!instance._comments.IsNullOrEmpty())
		{
			for(int i = 0; i < instance._comments.Length; i++)
			{
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
			var comment = _comments[linkIndex];
			string title = comment.number;
			string msg = comment.content;
			
			GameManager.Instance.ShowPopup(title, msg, position);
			
			return;
		}
		
		// highlight
		// copy
		// report
	}
}