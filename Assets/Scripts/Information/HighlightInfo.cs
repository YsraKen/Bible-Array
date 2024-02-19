using System;
using UnityEngine;
using ueRandom = UnityEngine.Random;

[Serializable]
public class HighlightInfo
{
	public string name;
	public Mark[] marks;
	
	public HighlightInfo(string name = null)
	{
		this.name = name;
		marks = new Mark[1]{ Mark.Default };
	}
	
	[Serializable]
	public struct Mark
	{
		public string name;
		public Color background;
		public Color letter;
		
		public bool b;
		public bool i;
		public bool s;
		public bool u;
		
		public static Mark Default => new Mark()
		{
			background = Color.yellow,
			letter = Color.black
		};
		
		public static Mark Random => new Mark
		{
			background = ueRandom.ColorHSV(),
			letter = ueRandom.ColorHSV(),
			b = ueRandom.value < 0.5f,
			i = ueRandom.value < 0.5f,
			s = ueRandom.value < 0.5f,
			u = ueRandom.value < 0.5f
		};
		
		public static Mark RandomComplimentary
		{
			get
			{
				var mark = new Mark();
				var hsv = new Vector3(ueRandom.value, ueRandom.value, ueRandom.value);
				
				mark.background = hsv.HsvToRgb();
				
				hsv.x += 0.5f;
				hsv.y += 0.5f;
				hsv.z += 0.5f;
				
				hsv.x = hsv.x % 1f;
				hsv.y = hsv.y % 1f;
				hsv.z = hsv.z % 1f;
				
				mark.letter = hsv.HsvToRgb();
				
				mark.b = ueRandom.value < 0.5f;
				mark.i = ueRandom.value < 0.5f;
				mark.s = ueRandom.value < 0.5f;
				mark.u = ueRandom.value < 0.5f;
				
				Debug.Log("Random Complimentary");
				return mark;
			}
		}
		
		public string GetBackgroundHex() => ColorUtility.ToHtmlStringRGBA(background);
		public string GetLetterHex() => ColorUtility.ToHtmlStringRGBA(letter);
		
		public bool Compare(Mark other)
		{
			bool output =
				(name == other.name) &&
				(background == other.background) &&
				(letter == other.letter) &&
				(b == other.b) &&
				(i == other.i) &&
				(s == other.s) &&
				(u == other.u);
			
			// Debug.Log($"is <b>'{name}'</b> equal to <b>'{other.name}'</b>? <color=yellow>{output}</color>");
			
			return output;
		}
	}
}