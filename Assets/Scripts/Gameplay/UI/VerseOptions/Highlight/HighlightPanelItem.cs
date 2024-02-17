using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighlightPanelItem : MonoBehaviour
{
	[SerializeField] private TMP_Text _headerTmp;
	[SerializeField] private TMP_InputField _headerInput;
	
	[Space]
	[SerializeField] private GameObject _elementTemplate;
	[SerializeField] private Transform _addElementButton;
	
	[Space]
	[SerializeField] private RectTransform _mainRect;
	[SerializeField] private GameObject _bodyRect;
	
	[Space]
	[SerializeField] private GameObject _foldoutExpandedArrow;
	[SerializeField] private GameObject _foldoutCollapsedArrow;
	[SerializeField] private VerticalLayoutGroup _layoutGroup;
	
	[Space]
	[SerializeField] private GameObject _selectedIndicator;
	[SerializeField] private GameObject _glow;
	
	int _index;
	List<GameObject> _elementInstances = new List<GameObject>();
	
	bool _isFoldoutExpanded;
	
	public void Init(int index, HighlightInfo info)
	{
		_index = index;
		_headerTmp.text = string.IsNullOrEmpty(info.name)? "<i>(Untitled)</i>": info.name;
		
		_elementTemplate.SetActive(true);
		
		var templateT = _elementTemplate.transform;
		var templateParent = templateT.parent;
		
		foreach(var mark in info.marks)
		{
			var element = Instantiate(_elementTemplate, templateParent, false);
			
			UpdateElementValues(element, mark);
			_elementInstances.Add(element);
		}
		
		templateT.SetAsLastSibling();
		_addElementButton.SetAsLastSibling();
		
		_elementTemplate.SetActive(false);
		
		_layoutGroup.Poke();
	}
	
	public static void UpdateElementValues(GameObject element, HighlightInfo.Mark mark, string noNameReplacement = "Sample Text")
	{
		var img = element.GetComponentInChildren<Image>();
			img.color = mark.background;
		
		var tmp = element.GetComponentInChildren<TMP_Text>();
			tmp.color = mark.letter;
			tmp.text = string.IsNullOrEmpty(mark.name)? noNameReplacement: mark.name;
		
		if(mark.b) addRichTextTag("<b>", "</b>");
		if(mark.i) addRichTextTag("<i>", "</i>");
		if(mark.s) addRichTextTag("<s>", "</s>");
		if(mark.u) addRichTextTag("<u>", "</u>");
		
		void addRichTextTag(string a, string b) => tmp.text = $"{a}{tmp.text}{b}";
	}
	
	public void SetSelected(bool isSelected = true) => _selectedIndicator.SetActive(isSelected);
	public void Ping() => _glow.SetActive(true);
	
	public void ToggleFoldout()
	{
		_isFoldoutExpanded = !_isFoldoutExpanded;
		SetFoldout(_isFoldoutExpanded);
	}
	
	public void SetFoldout(bool isExpanded)
	{
		_isFoldoutExpanded = isExpanded;

		_foldoutExpandedArrow.SetActive(_isFoldoutExpanded);
		_foldoutCollapsedArrow.SetActive(!_isFoldoutExpanded);
		
		_bodyRect.SetActive(_isFoldoutExpanded);
		
		_layoutGroup.Poke();
	}
	
	public void OnEdit()
	{
		_headerInput.SetTextWithoutNotify(_headerTmp.text);
		
		_headerTmp.gameObject.SetActive(false);
		_headerInput.gameObject.SetActive(true);
	}
	
	public void OnEditInput(string value)
	{
		_headerTmp.text = value;
		
		_headerTmp.gameObject.SetActive(true);
		_headerInput.gameObject.SetActive(false);
		
		HighlightPanel.Instance.OnItemEdit(value, _index);
	}
	
	public void OnDelete()
	{
		if(HighlightPanel.Instance.Infos.Count == 1)
			return;
		
		HighlightPanel.Instance.OnItemDelete(this);
		Destroy(gameObject);
	}
	
	public void OnElementEdit(Transform transform)
	{
		int index = transform.GetSiblingIndex();
		HighlightPanel.Instance.OpenColorPanel(_index, index, onApply);
		
		void onApply(HighlightInfo.Mark mark)
		{
			var element = _elementInstances[index];
			UpdateElementValues(element, mark);
		}
	}
	
	public void OnElementDelete(Transform transform)
	{
		int index = transform.GetSiblingIndex();
		var info = HighlightPanel.Instance.GetActiveInfo();
		
		var marks = info.marks.ToList();
			marks.RemoveAt(index);
		
		info.marks = marks.ToArray();
		
		_elementInstances.RemoveAt(index);
		Destroy(transform.gameObject);
	}
	
	public void AddElement()
	{
		var templateT = _elementTemplate.transform;
		
		var newElement = Instantiate(_elementTemplate, templateT.parent, false);
			newElement.gameObject.SetActive(true);
		
		var newMark = HighlightInfo.Mark.Default;
		UpdateElementValues(newElement, newMark);
		
		var info = HighlightPanel.Instance.GetActiveInfo();
		var marks = info.marks.ToList();
		
		marks.Add(newMark);
		info.marks = marks.ToArray();
		
		_elementInstances.Add(newElement);
		
		templateT.SetAsLastSibling();
		_addElementButton.SetAsLastSibling();
		
		_layoutGroup.Poke();
	}
}