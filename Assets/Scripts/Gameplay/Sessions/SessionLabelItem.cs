using UnityEngine;
using UnityEngine.UI;
using System;

public class SessionLabelItem : MonoBehaviour
{
	[SerializeField] private GameObject _highlight;
	[SerializeField] private Text _labelTxt;
	[SerializeField] private InputField _labelInput;
	
	[Space]
	[SerializeField] private GameObject[] _onDefaultModeObjects;
	[SerializeField] private GameObject[] _onEditModeObjects;
	
	public int Index { get; private set; }
	
	public void Init(int index, string label)
	{
		Index = index;
		
		_labelTxt.text = label;
		_labelInput.SetTextWithoutNotify(label);
	}
	
	public void OnSelect() => SessionSelect.Instance.OnLabelSelect(Index);
	
	public void SetHighlight(bool isHighlighted) => _highlight.SetActive(isHighlighted);
	
	public void OnEditStart()
	{
		Array.ForEach(_onDefaultModeObjects, obj => obj.SetActive(false));
		Array.ForEach(_onEditModeObjects, obj => obj.SetActive(true));
		
		_labelInput.Select();
	}
	
	public void OnEdit(string value)
	{
		_labelTxt.text = value;
		SessionSelect.Instance.OnLabelEdit(Index, value);
		
		OnEditEnd();
	}
	
	public void OnEditEnd()
	{
		Array.ForEach(_onEditModeObjects, obj => obj.SetActive(false));
		Array.ForEach(_onDefaultModeObjects, obj => obj.SetActive(true));
	}
	
	public void Delete() => SessionSelect.Instance.OnLabelDelete(Index);
}