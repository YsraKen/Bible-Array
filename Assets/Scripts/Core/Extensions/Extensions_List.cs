using System.Collections.Generic;
using System;
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
	
	/// <summary>
	/// Loop two lists together
	/// </summary>
	/// <param name ="others">The secondary list you want to loop synchronously</param>
	/// <param name ="action">Invoke a logic with the current elements of both lists (T and U) along with the current iteration index (int)</param>
	/// <param name ="breaker">Optional, break the iteration if invoking return is true</param>
	public static void ForWithOther<T, U>
	(
		this List<T> currents,
		ref List<U> others,
		Action<T, U, int> action,
		Func<bool> breaker = null
	){
		int count = currents.Count;
		
		for(int i = 0; i < count; i++)
		{
			action(currents[i], others.GetElement(i), i);
			
			if(breaker != null)
				if(breaker()) break;
		}
	}
	
	/* public T Add<T>(List<T> list, T t, out int index)
	{
		list.Add(t);
		
		index = list.Count - 1;
		return t;
	} */
	
	public static bool IsInsideRange<T>(this List<T> list, int index) => index > -1 && index < list.Length();
}