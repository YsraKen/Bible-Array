using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightPanel : MonoBehaviour
{
	[SerializeField] private HighlightPanel _itemTemplate;
	[SerializeField] private GameObject _colorSelectPanel;
	
	[field: SerializeField]
	public List<HighlightInfo> Infos { get; private set; } = new List<HighlightInfo>();
	
	[SerializeField, EnumData(typeof(ScreenOrientation))]
	private RectTransform[] _screenOrientationRefs;
	
	[SerializeField] private RectTransform _panel;
	
	void OnEnable()
	{
		UpdatePanelOrientation(GameManager.Instance.ScreenOrientation);
	}
	
	public void UpdatePanelOrientation(ScreenOrientation orientation)
	{
		var screenRef = _screenOrientationRefs[(int) orientation];
		
		_panel.position = screenRef.position;
		_panel.sizeDelta = screenRef.sizeDelta;
	}
}

[Serializable]
public class HighlightInfo
{
	public string name;
	public ColorCombination[] colors;
	
	[Serializable]
	public struct ColorCombination
	{
		public string name;
		public Color background;
		public Color letter;
	}
}