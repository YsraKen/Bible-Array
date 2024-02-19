using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPanel : MonoBehaviour
{
	#region Variables
	
	[SerializeField] private TMP_InputField _nameInput;
	[SerializeField] private Image _nameInputBackground;
	[SerializeField] private TMP_Text _nameInputLetter;
	
	[Space]
	[SerializeField] private Image _backgroundColorField;
	[SerializeField] private Image _letterColorField;
	
	[SerializeField] private TMP_Text _backgroundHexTmp;
	[SerializeField] private TMP_Text _letterHexTmp;
	
	[Space]
	[SerializeField] private GameObject _colorSelect;
	[SerializeField] private Image[] _hueSelect;
	
	[Space]
	[SerializeField] private GameObject[] _colorSaturationSliderPings;
	[SerializeField] private GameObject[] _colorValueSliderPings;
	
	[Space]
	[SerializeField] private Image _colorSelectPreview;
	[SerializeField] private TMP_InputField _colorSelectPreviewHexInput;
	
	[Space]
	[SerializeField] private Toggle _bToggle;
	[SerializeField] private Toggle _iToggle;
	[SerializeField] private Toggle _sToggle;
	[SerializeField] private Toggle _uToggle;

	public string nameInput { get; set; }
	
	Vector3Int _selectedHsvIndex;
	
	Action<HighlightInfo.Mark> _onSubmit;
	
	#endregion
	
	#region Methods
	
	void OnDisable()
	{
		var mark = new HighlightInfo.Mark()
		{
			name = nameInput,
			background = _backgroundColorField.color,
			letter = _letterColorField.color,
			b = _bToggle.isOn,
			i = _iToggle.isOn,
			s = _sToggle.isOn,
			u = _uToggle.isOn
		};
		
		_onSubmit(mark);
		_onSubmit = null;
		
		_nameInput.SetTextWithoutNotify("");
		nameInput = "";
	}
	
	public void Open(HighlightInfo.Mark mark, Action<HighlightInfo.Mark> onSubmit)
	{
		nameInput = mark.name;
		_nameInput.SetTextWithoutNotify(mark.name);
		
		_backgroundColorField.color = mark.background;
		_letterColorField.color = mark.letter;
		
		_backgroundHexTmp.text = ColorUtility.ToHtmlStringRGB(mark.background);
		_letterHexTmp.text = ColorUtility.ToHtmlStringRGB(mark.letter);
		
		_nameInputBackground.color = mark.background;
		_nameInputLetter.color = mark.letter;
		
		_bToggle.SetIsOnWithoutNotify(mark.b);
		_iToggle.SetIsOnWithoutNotify(mark.i);
		_sToggle.SetIsOnWithoutNotify(mark.s);
		_uToggle.SetIsOnWithoutNotify(mark.u);
		
		UpdateNameInputLetter_BISU(mark.b, mark.i, mark.s, mark.u);
		
		_onSubmit = onSubmit;
		
		gameObject.SetActive(true);
		_nameInput.Select();
	}
	
	public void OnBISUToggle()
	{
		UpdateNameInputLetter_BISU
		(
			_bToggle.isOn,
			_iToggle.isOn,
			_sToggle.isOn,
			_uToggle.isOn
		);
	}
	
	private void UpdateNameInputLetter_BISU(bool b, bool i, bool s, bool u)
	{
		_nameInput.SetTextWithoutNotify(nameInput);
		_nameInputLetter.fontStyle = FontStyles.Normal;
		
		if(b) _nameInputLetter.fontStyle |= (FontStyles)(1 << 0);
		if(i) _nameInputLetter.fontStyle |= (FontStyles)(1 << 1);
		if(s) _nameInputLetter.fontStyle |= (FontStyles)(1 << 6);
		if(u) _nameInputLetter.fontStyle |= (FontStyles)(1 << 2);
	}
	
	public void SelectBackgroundColor()
	{
		ColorSelect(_backgroundColorField.color, value =>
		{
			_backgroundColorField.color = value;
			_backgroundHexTmp.text = ColorUtility.ToHtmlStringRGB(value);
			
			_nameInputBackground.color = value;
		});
	}
	
	public void SelectLetterColor()
	{
		ColorSelect(_letterColorField.color, value =>
		{
			_letterColorField.color = value;
			_letterHexTmp.text = ColorUtility.ToHtmlStringRGB(value);
			
			_nameInputLetter.color = value;
		});
	}
	
	public void ColorSelect(Color initialColor, Action<Color> onFinish)
	{
		var hsv = initialColor.RgbToHsv();
		
		_selectedHsvIndex.x = Mathf.RoundToInt(Mathf.Lerp(0, hues.Length - 1, hsv.x));
		_selectedHsvIndex.y = Mathf.RoundToInt(Mathf.Lerp(0, 4, hsv.y));
		_selectedHsvIndex.z = Mathf.RoundToInt(Mathf.Lerp(0, 4, hsv.z));
		
		UpdateSelectedHue();
		UpdateSatValUI();
		UpdateHexValue();
		
		StartCoroutine(r());
		IEnumerator r()
		{
			yield return null;
			
			_colorSelect.SetActive(true);
			yield return new WaitWhile(()=> _colorSelect.activeSelf);
			
			onFinish(_colorSelectPreview.color);
		}
	}
	
	public void OnHexInput(string value)
	{
		var color = _colorSelectPreview.color;
		
		if(ColorUtility.TryParseHtmlString("#" + value, out var inputColor))
			color = inputColor;
		
		_colorSelectPreview.color = color;
		
		string hex = ColorUtility.ToHtmlStringRGB(color);
		_colorSelectPreviewHexInput.SetTextWithoutNotify(hex);
		
		var hsv = color.RgbToHsv();
		
		_selectedHsvIndex.x = Mathf.RoundToInt(Mathf.Lerp(0, hues.Length - 1, hsv.x));
		_selectedHsvIndex.y = Mathf.RoundToInt(Mathf.Lerp(0, 4, hsv.y));
		_selectedHsvIndex.z = Mathf.RoundToInt(Mathf.Lerp(0, 4, hsv.z));
		
		UpdateSelectedHue();
		UpdateSatValUI();
	}
	
	public void OnHueSelect(Transform transform)
	{
		_selectedHsvIndex.x = transform.GetSiblingIndex();
		
		UpdateSelectedHue();
		UpdateSatValUI();
		UpdateHexValue();
	}
	
	#endregion
	
	public Image hueTemplate;
	public Image[] hues;
	public Image[] satVals;
	
	[ContextMenu("Setup Hues")]
	void SetupHues()
	{
		int count = hues.Length;
		
		for(int i = 0; i < count; i++)
		{
			if(hues[i])
				DestroyImmediate(hues[i].transform.parent.gameObject);
		}
		
		var template = hueTemplate.transform.parent;
			template.gameObject.SetActive(true);
		
		int maxHueIndex = count - 1;
		
		for(int i = 0; i < maxHueIndex; i++)
		{
			float lerp = Mathf.InverseLerp(0, maxHueIndex, i);
			
			var obj = Instantiate(template, template.parent, false);
				obj.eulerAngles = Vector3.back * (lerp * 360);
			
			var child = obj.GetChild(0);
				child.rotation = Quaternion.identity;
			
			var img = child.GetComponent<Image>();
				img.color = new Vector3(lerp, 1, 1).HsvToRgb();
			
			hues[i] = img;
		}
		
		template.SetAsLastSibling();
		template.gameObject.SetActive(false);
		
		UpdateSelectedHue();
		UpdateSatValUI();
	}
	
	void UpdateSelectedHue()
	{
		for(int i = 0; i < hues.Length - 1; i++)
			hues[i].transform.GetChild(0).gameObject.SetActive(i == _selectedHsvIndex.x);
	}
	
	void UpdateSatValUI()
	{
		float h = Mathf.InverseLerp(0, hues.Length - 1, _selectedHsvIndex.x);
		
		foreach(var img in satVals)
		{
			var sTransform =  img.transform;
			var vTransform = img.transform.parent;
			
			int sIndex = sTransform.GetSiblingIndex();
			int vIndex = vTransform.GetSiblingIndex();
			
			float s = Mathf.InverseLerp(0, vTransform.childCount - 1, sIndex);
			float v = Mathf.InverseLerp(0, vTransform.parent.childCount - 1, vIndex);
			
			img.color = Color.HSVToRGB(h, s, v);
		}
		
		for(int i = 0; i < satVals.Length; i++)
		{
			var transform = satVals[i].transform;
			
			int sIndex = transform.GetSiblingIndex();
			int vIndex = transform.parent.GetSiblingIndex();
			
			bool isSelected = _selectedHsvIndex.y == sIndex && _selectedHsvIndex.z == vIndex;
			
			transform.GetChild(0).gameObject.SetActive(isSelected);
		}
	}
	
	void UpdateHexValue()
	{
		var previewColor = Color.HSVToRGB
		(
			Mathf.InverseLerp(0, hues.Length - 1, _selectedHsvIndex.x),
			Mathf.InverseLerp(0, 4, _selectedHsvIndex.y),
			Mathf.InverseLerp(0, 4, _selectedHsvIndex.z)
		);
		
		var previewHex = ColorUtility.ToHtmlStringRGB(previewColor);
		
		_colorSelectPreview.color = previewColor;
		_colorSelectPreviewHexInput.SetTextWithoutNotify(previewHex);
	}
	
	public void OnSaturationAndValueSelect(Transform transform)
	{
		_selectedHsvIndex.y = transform.GetSiblingIndex();
		_selectedHsvIndex.z = transform.parent.GetSiblingIndex();
		
		UpdateSelectedHue();
		UpdateSatValUI();
		UpdateHexValue();
	}
}