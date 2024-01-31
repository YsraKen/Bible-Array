using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public static partial class Extensions
{
	/// <summary>
	/// Check if array is null or array has a size of 0
	/// </summary>
	public static bool IsNullOrEmpty(this Array array) => (array == null || array.Length == 0);
	
	/// <summary>
	/// Returns 0 if array is null
	/// </summary>
	public static int Length(this Array array) => (array == null)? 0: array.Length;
	
	/// <summary>
	/// Returns default if index is out of range
	/// </summary>
	public static T GetElement<T>(this T[] array, int index)
	{
		if(array == null)
			return default;
		
		else if(index >= 0 && index < array.Length)
			return array[index];
		
		else return default;
	}
	
	/// <summary>
	/// Gets the element of the specified type, if it exists.
	/// </summary>
	/// <param name ="index">Index of the element to return.</param>
	/// <param name ="element">The output argument that will contain the element or default.</param>
	/// <returns>Returns true if the element is found, false otherwise.</returns>
	public static bool TryGetElement<T>(this T[] array, int index, out T element)
	{
		element = array.GetElement(index);
		bool isDefault = EqualityComparer<T>.Default.Equals(element, default);
		
		return !isDefault;
	}
	
	/// <summary>
	/// Get random element from array
	/// </summary>
	public static T GetRandom<T>(this T[] array) => array.GetElement(Random.Range(0, array.Length));
	
	/// <summary>
	/// Get random element from array with index
	/// </summary>
	public static T GetRandom<T>(this T[] array, out int index)
	{
		index = Random.Range(0, array.Length);
		return array.GetElement(index);
	}
	
	/// <summary>
	/// Loop two arrays together
	/// </summary>
	/// <param name ="others">The secondary array you want to loop synchronously</param>
	/// <param name ="action">Invoke a logic with the current elements of both array (T and U) along with the current iteration index</param>
	/// <param name ="breaker">Optional, break the iteration if invoking return is true</param>
	public static void ForWithOther<T, U>
	(
		this T[] currents,
		ref U[] others,
		Action<T, U, int> action,
		Func<bool> breaker = null
	){
		int length = currents.Length;
		
		for(int i = 0; i < length; i++)
		{
			T current = currents[i];
			U other = others.GetElement(i);
			
			action(current, other, i);
			
			if(breaker != null)
				if(breaker()) break;
		}
	}
	
	// Current Use: for Collectibles data, no null and range checking
	static void ExpandArrayWithIndex<T>(ref T[] array, int index)
	{
		int count = array.Length();
		
		if(count <= index)
		{
			var list = array == null? new List<T>(): new List<T>(array);
			int difference = (index + 1) - count; // "+1" array offset
			
			for(int i = 0; i < difference; i++)
				list.Add(default(T));
			
			array = list.ToArray();
		}
	}
	
	static T ExpandArrayWithIndex<T>(ref T[] array, int index, T manuallyCreatedInstance)
	{
		int count = array.Length();
		
		if(count <= index)
		{
			var list = array == null? new List<T>(): new List<T>(array);
			int difference = (index + 1) - count; // "+1" array offset
			
			for(int i = 0; i < difference; i++)
				list.Add(default(T));
			
			array = list.ToArray();
		}
		
		array[index] = manuallyCreatedInstance;
		return manuallyCreatedInstance;
	}
	
	/* static T ArrayFind<T>(this T[] array, Predicate<T> predicate)
	{
		if(array.IsNullOrEmpty())
			return default(T);
		
		foreach(var element in array)
		{
			if(predicate
		}
	} */
	
	public static bool IsInsideRange<T>(this T[] array, int index) => index > -1 && index < array.Length();
}