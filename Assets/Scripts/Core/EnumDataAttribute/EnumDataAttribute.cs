using UnityEngine;
using System;

public class EnumDataAttribute : PropertyAttribute
{
	public string[] names{ get; }
	
	public EnumDataAttribute(Type enumType) =>
		names = Enum.GetNames(enumType);
}