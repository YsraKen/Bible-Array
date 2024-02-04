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
	
	public static bool IsInsideRange<T>(this T[] array, int index) => index > -1 && index < array.Length();
}