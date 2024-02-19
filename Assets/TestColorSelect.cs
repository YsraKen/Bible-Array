using UnityEngine;
using UnityEngine.UI;

public class TestColorSelect : MonoBehaviour
{
	const int HUE_CNT = 20;
	
	[Range(0, HUE_CNT)]
	public int hue;
	
	public Image[] svImgs;
	
	[ContextMenu("Update Sv")]
	void UpdateSv()
	{}
}