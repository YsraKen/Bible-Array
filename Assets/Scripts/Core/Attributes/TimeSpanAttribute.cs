using UnityEngine;

public enum TimeSpanType
{
	Second = 1,
	Minute = 60,
	Hour = 3600,
	Day = 86400
}

public class TimeSpanAttribute : PropertyAttribute
{	
	public TimeSpanType type;
	public string labelOverride { get; }
	
	public TimeSpanAttribute(TimeSpanType type = default, string labelOverride = null)
	{
		this.type = type;
		this.labelOverride = labelOverride;
	}
}