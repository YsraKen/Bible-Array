using UnityEngine;

public class PreviewImageAttribute : PropertyAttribute
{
	public float size{ get; set; } = 50;
	public bool isExpanded{ get; set; } = true;
}