using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookInfoUI : MonoBehaviour
{
	[SerializeField] private RectTransform _panel;
	
	[SerializeField, EnumData(typeof(ScreenOrientation))]
	private RectTransform[] _panelOrientationRefs;
	
	[SerializeField] private TMP_Text _titleTmp;
	[SerializeField] private TMP_Text _bodyTmp;
	[SerializeField] private ScrollRect _scroll;
	
	void OnValidate()
	{
		if(Application.isPlaying)
			return;
		
		var mgr = FindObjectOfType<GameManager>();
		
		AdjustPanel(mgr.ScreenOrientation);
	}
	
	void OnEnable()
	{
		AdjustPanel(GameManager.Instance.ScreenOrientation);
		
		_scroll.verticalNormalizedPosition = 1f;
	}
	
	public void AdjustPanel(ScreenOrientation orientation)
	{
		var target = _panelOrientationRefs[(int) orientation];
		
		_panel.sizeDelta = target.sizeDelta;
		_panel.position = target.position;
	}
	
	public void Show(string text, string title = "Book Information")
	{
		_titleTmp.text = title;
		_bodyTmp.text = text;
		
		gameObject.SetActive(true);
	}
}