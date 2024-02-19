using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class Tools
{
	public static void AddToArray<T>(ref T[] array, T t)
	{
		var list = (array == null)? new List<T>(): array.ToList();
			list.Add(t);
		
		array = list.ToArray();
	}
	
	public static void RemoveToArray<T>(ref T[] array, int index)
	{
		var list = (array == null)? new List<T>(): array.ToList();
			list.RemoveAt(index);
		
		array = list.ToArray();
	}
	
}