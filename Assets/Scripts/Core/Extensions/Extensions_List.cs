using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static partial class Extensions
{
	/// <summary>
	/// Check if list is null or list has a size of 0
	/// </summary>
	public static bool IsNullOrEmpty<T>(this List<T> list) => (list == null || list.Count == 0);
	
	/// <summary>
	/// Returns 0 if list is null
	/// </summary>
	public static int Length<T>(this List<T> list) => (list == null)? 0: list.Count;
	
	/// <summary>
	/// Returns default if index is out of range
	/// </summary>
	public static T GetElement<T>(this List<T> list, int index)
	{
		if(list == null)
			return default;
		
		else if(index >= 0 && index < list.Count)
			return list[index];
		
		else return default;
	}
	
	public static T GetLastElement<T>(this List<T> list) => list.IsNullOrEmpty()? default(T): list[list.Count - 1];
	
	/// <summary>
	/// Gets the element of the specified type, if it exists.
	/// </summary>
	/// <param name ="index">Index of the element to return.</param>
	/// <param name ="element">The output argument that will contain the element or default.</param>
	/// <returns>Returns true if the element is found, false otherwise.</returns>
	public static bool TryGetElement<T>(this List<T> list, int index, out T element)
	{
		element = list.GetElement(index);
		bool isDefault = EqualityComparer<T>.Default.Equals(element, default);
		
		return !isDefault;
	}
	
	/// <summary>
	/// Get a random element from list
	/// </summary>
	public static T GetRandom<T>(this List<T> list) => list.GetElement(Random.Range(0, list.Count));
	
	/// <summary>
	/// Get a random element from list with index
	/// </summary>
	public static T GetRandom<T>(this List<T> list, out int index)
	{
		index = Random.Range(0, list.Count);
		return list.GetElement(index);
	}
	
	public static List<T> GetRandom<T>(this List<T> pool, int count)
	{
		var output = new List<T>();
		
		count = Mathf.Clamp(count, 0, pool.Count);
		
		for(int i = 0; i < count; i++)
		{
			int index = Random.Range(0, pool.Count);
			
			output.Add(pool[index]);
			pool.RemoveAt(index);
		}
		
		return output;
	}
	
	public static T Add<T>(List<T> list, T t, out int index)
	{
		list.Add(t);
		
		index = list.Count - 1;
		return t;
	}
	
	public static T Add<T>(List<T> list, T t)
	{
		list.Add(t);
		return t;
	}
	
	public static bool IsInsideRange<T>(this List<T> list, int index) => index > -1 && index < list.Length();
	
	public static bool TryFind<T>(this List<T> list, Predicate<T> predicate, out T element)
	{
		int index = list.FindIndex(predicate);
		bool found = index != -1;
		
		element = found? list[index]: default(T);
		
		return found;
	}
	
	public static bool TryFindIndex<T>(this List<T> list, Predicate<T> predicate, out int index)
	{
		index = list.FindIndex(predicate);
		return index != -1;
	}
	
	public static bool Contains<T>(this List<T> list, T element, out int index)
	{
		index = list.FindIndex(e => EqualityComparer<T>.Default.Equals(e, element));
		return index != -1;
	}
}