using UnityEngine;

public class FlagAttribute : PropertyAttribute
{
	public string groupName { get; }
	
	public FlagAttribute(string groupName) => this.groupName = groupName;
}