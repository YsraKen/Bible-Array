using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public static partial class Extensions
{
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
}