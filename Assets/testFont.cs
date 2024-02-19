using System;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class testFont : MonoBehaviour
{
	public TMP_Text tmp;
	public FontStyles fontStyle;
	
	[Flags]
	public enum Huh
	{
		None = 0,
		A = 1 << 0,   // 0001
		B = 1 << 1,   // 0010
		C = 1 << 2,   // 0100
		D = 1 << 3    // 1000
	}
	
	public Huh huh;
	
	void OnValidate()
	{
		if(!tmp)
			tmp = GetComponent<TMP_Text>();
		
		tmp.fontStyle = fontStyle;
	}
	
	[ContextMenu("Bold")]
	public void ToggleBold() => fontStyle ^= FontStyles.Bold;
	
	[ContextMenu("Italic")]
    public void ToggleItalic() => fontStyle ^= FontStyles.Italic;

	[ContextMenu("Underline")]
    public void ToggleUnderline() => fontStyle ^= FontStyles.Underline;
	
	[ContextMenu("Strikethrough")]
    public void ToggleStrikethrough() => fontStyle ^= FontStyles.Strikethrough;
	
	[ContextMenu("A")] public void A() => huh ^= Huh.A;
	[ContextMenu("B")] public void B() => huh ^= Huh.B;
	[ContextMenu("C")] public void C() => huh ^= Huh.C;
	[ContextMenu("D")] public void D() => huh ^= Huh.D;
}