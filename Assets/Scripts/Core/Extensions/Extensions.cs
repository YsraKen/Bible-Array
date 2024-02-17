using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public static partial class Extensions
{
	#region ScrollRect
	
	public static void SetPosition(this ScrollRect scroll, Vector2 normalizedValue, float duration, Action onFinish = null)
	{
		scroll.StartCoroutine(r());
		IEnumerator r()
		{
			float timer = 0f;
			var startPos = scroll.normalizedPosition;
			
			while(timer <= duration)
			{
				yield return null;
				
				timer += Time.deltaTime;
				
				scroll.normalizedPosition = Vector2.Lerp(startPos, normalizedValue, timer / duration);
			}
			
			scroll.normalizedPosition = normalizedValue;
			onFinish?.Invoke();
		}
	}
	
	public static IEnumerator SetPositionRoutine(this ScrollRect scroll, Vector2 normalizedValue, float duration, Action onFinish = null)
	{
		float timer = 0f;
		var startPos = scroll.normalizedPosition;
		
		while(timer <= duration)
		{
			yield return null;
			
			timer += Time.deltaTime;
			
			scroll.normalizedPosition = Vector2.Lerp(startPos, normalizedValue, timer / duration);
		}
		
		scroll.normalizedPosition = normalizedValue;
		onFinish?.Invoke();
	}
	
	#endregion
	
	#region Color
	
	public static Vector3 RgbToHsv(this Color color)
	{
		var output = Vector3.zero;
		Color.RGBToHSV(color, out output.x, out output.y, out output.z);
		
		return output;
	}
	
	public static Vector3 ToRgb(this Color color) => new Vector3(color.r, color.g, color.b);
	
	public static Color HsvToRgb(this Vector3 hsv) => Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
	
	public static Vector3 RgbToHsv(this Vector3 rgb)
	{
		var color = new Color(rgb.x, rgb.y, rgb.z, 1f);
		
		return color.RgbToHsv();
	}
	
	#endregion

	public static void Poke(this HorizontalOrVerticalLayoutGroup layoutGroup)
	{
		layoutGroup.StartCoroutine(r());
		IEnumerator r()
		{
			yield return null;
			layoutGroup.childForceExpandHeight = !layoutGroup.childForceExpandHeight;
			yield return null;
			layoutGroup.childForceExpandHeight = !layoutGroup.childForceExpandHeight;
		}
	}
}