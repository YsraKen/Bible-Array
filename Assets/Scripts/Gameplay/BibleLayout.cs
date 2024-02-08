using UnityEngine;
using UnityEngine.UI;
using System;

public class BibleLayout : MonoBehaviour
{
	[ContextMenu("Test")]
	public void Repaint()
	{
		var bibles = GameManager.Instance.BibleInstances;
		if(bibles.IsNullOrEmpty()) return;
		
		Array.ForEach(bibles, bible => bible.UpdateVerseSizes());
		
		int numberOfVerses = bibles[0].Body.childCount;
		float maxVerseSize = 0;
	
		for(int i = 0; i < numberOfVerses; i++)
		{
			foreach(var bible in bibles)
			{
				bool isInsideRange = i > -1 && i < bible.Body.childCount;
				if(!isInsideRange) continue;
				
				var verse = bible.Body.GetChild(i) as RectTransform;
				float verseSize = verse.sizeDelta.y;
				
				if(verseSize > maxVerseSize)
					maxVerseSize = verseSize;
			}
			
			foreach(var bible in bibles)
			{
				bool isInsideRange = i > -1 && i < bible.Body.childCount;
				if(!isInsideRange) continue;
			
				var verse = bible.Body.GetChild(i) as RectTransform;
					verse.sizeDelta = new Vector2(verse.sizeDelta.x, maxVerseSize);
			}
			
			maxVerseSize = 0;
		}
		
		Array.ForEach(bibles, bible => bible.AdjustBodySize());
	}
}