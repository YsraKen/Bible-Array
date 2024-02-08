using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VersionSelectPanel : MonoBehaviour
{
	public RectTransform panel;
	private Vector2 _originalSize;
	
	void OnEnable()
	{
		if(_originalSize == Vector2.zero)
			_originalSize = panel.sizeDelta;
	
		var size = _originalSize;
		var screenWidth = GameManager.Instance.ScreenSize.x;
		
		if(size.x > screenWidth)
			size.x = screenWidth;
		
		panel.sizeDelta = size;
	}
}