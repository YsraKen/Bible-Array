using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ColorPanel : MonoBehaviour
{
	[SerializeField] private InputField _nameInput;
	[SerializeField] private Image _backgroundColorField;
	[SerializeField] private Image _letterColorField;
	
	[Space]
	[SerializeField] private GameObject _colorSelect;
	[SerializeField] private Image[] _hueSelect;
	[SerializeField] private Slider _colorSaturationSlider;
	[SerializeField] private Slider _colorValueSlider;
	
	[Space]
	[SerializeField] private Image _colorSelectPreview;
	[SerializeField] private Image _colorSaturationPreview;
	[SerializeField] private Image _colorValuePreview;
	
	[Space]
	[SerializeField] private Toggle _bToggle;
	[SerializeField] private Toggle _iToggle;
	[SerializeField] private Toggle _sToggle;
	[SerializeField] private Toggle _uToggle;

	int _selectedHueIndex;
	Action<HighlightInfo.Mark> _onSubmit;
	
	void OnDisable()
	{
		var mark = new HighlightInfo.Mark()
		{
			name = _nameInput.text,
			background = _backgroundColorField.color,
			letter = _letterColorField.color,
			b = _bToggle.isOn,
			i = _iToggle.isOn,
			s = _sToggle.isOn,
			u = _uToggle.isOn
		};
		
		_onSubmit(mark);
		_onSubmit = null;
	}
	
	public void Open(HighlightInfo.Mark mark, Action<HighlightInfo.Mark> onSubmit)
	{
		_nameInput.SetTextWithoutNotify(mark.name);
		
		_backgroundColorField.color = mark.background;
		_letterColorField.color = mark.letter;
		
		_bToggle.SetIsOnWithoutNotify(mark.b);
		_iToggle.SetIsOnWithoutNotify(mark.i);
		_sToggle.SetIsOnWithoutNotify(mark.s);
		_uToggle.SetIsOnWithoutNotify(mark.u);
		
		_onSubmit = onSubmit;
		
		gameObject.SetActive(true);
		_nameInput.Select();
	}
	
	public void SelectBackgroundColor() => ColorSelect(_backgroundColorField.color, value => _backgroundColorField.color = value);
	public void SelectLetterColor() => ColorSelect(_letterColorField.color, value => _letterColorField.color = value);
	
	public void ColorSelect(Color initialColor, Action<Color> onFinish)
	{
		var hsv = initialColor.RgbToHsv();
		
		_selectedHueIndex = Mathf.RoundToInt(((float) _hueSelect.Length - 1) * hsv.x);
		
		_colorSaturationSlider.SetValueWithoutNotify(hsv.y);
		_colorValueSlider.SetValueWithoutNotify(hsv.z);
		
		_colorSelectPreview.color = initialColor;
		
		_colorSaturationPreview.color = Color.HSVToRGB(hsv.x, 1, 1);
		_colorValuePreview.color = Color.HSVToRGB(hsv.x, hsv.y, 1);
	
		StartCoroutine(r());
		IEnumerator r()
		{
			yield return null;
			
			_colorSelect.SetActive(true);
			yield return new WaitWhile(()=> _colorSelect.activeSelf);
			
			onFinish(_colorSelectPreview.color);
		}
	}
	
	public void OnHueSelect(Transform transform)
	{
		_selectedHueIndex = transform.GetSiblingIndex();
		
		UpdateColorPreview();
	}
	
	public void UpdateColorPreview()
	{
		float hueValue = Mathf.InverseLerp(0, _hueSelect.Length - 1, _selectedHueIndex);
		
		var hsv = new Vector3
		(
			hueValue,
			_colorSaturationSlider.value,
			_colorValueSlider.value
		);
		
		_colorSelectPreview.color = hsv.HsvToRgb();
		
		_colorSaturationPreview.color = Color.HSVToRGB(hsv.x, 1, 1);
		_colorValuePreview.color = Color.HSVToRGB(hsv.x, hsv.y, 1);
	}
	
	[ContextMenu("Colorize Hue Buttons")]
	void ColorizeHueButtons()
	{
		int count = _hueSelect.Length;
		
		for(int i = 0; i < count; i++)
		{
			var hsv = new Vector3(Mathf.InverseLerp(0, count - 1, i), 1f, 1f);
			_hueSelect[i].color = hsv.HsvToRgb();
		}
	}
}