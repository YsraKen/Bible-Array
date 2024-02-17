using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BibleCategory : MonoBehaviour
{
	public GameObject highlight;
	public GameObject[] subCategories;
	
	[Space]
	public IntPopup[] books;
	public Transform booksParent;
	
	public bool dontDeactivateSelf;
	public bool dontDeactivateOthers;
	
	private static List<BibleCategory> _instances = new List<BibleCategory>();
	private static Coroutine _bookToggleRoutine;
	
	void Awake() => _instances.Add(this);
	void OnDestroy() => _instances.Remove(this);
	
/* 	// public GeneralInformation genInfo;
	// public IntPopup[] range = new IntPopup[2];
	
	void OnValidate()
	{
		if(!genInfo) return;
		
		for(int r = 0; r < range.Length; r ++)
		{
			int count = genInfo.bookChapterVerseInfos.Length;
			range[r].options = new string[count];
			
			for(int i = 0; i < count; i++)
				range[r].options[i] = genInfo.bookChapterVerseInfos[i].name;
		}
		
		{
			int n = (range[1] - range[0]) + 1;
			int r = range[0];
			
			books = new IntPopup[n];
			
			for(int i = 0; i < n; i++)
			{
				books[i] = r ++;
			}
		}
		
		for(int b = 0; b < books.Length; b ++)
		{
			int count = genInfo.bookChapterVerseInfos.Length;
			books[b].options = new string[count];
			
			for(int i = 0; i < count; i++)
				books[b].options[i] = genInfo.bookChapterVerseInfos[i].name;
		}
	} */
	
	public void ToggleObjects()
	{
		foreach(var instance in _instances)
		{
			instance.highlight.SetActive(false);
			
			if(dontDeactivateOthers)
				continue;
			
			if(!instance.dontDeactivateSelf)
				instance.gameObject.SetActive(instance == this);
		}
		
		foreach(var sc in subCategories)
			sc.SetActive(true);
		
		highlight.SetActive(true);
		
		if(_bookToggleRoutine != null)
			StopCoroutine(_bookToggleRoutine);
		
		_bookToggleRoutine = StartCoroutine(r());
		
		IEnumerator r()
		{
			for(int i = 0; i < booksParent.childCount; i++)
			{
				// if(i == Navigator.SelectedBookIndex)
					// continue;
				
				var child = booksParent.GetChild(i).gameObject;
					child.SetActive(false);
			}
			
			var step = new WaitForSeconds(0.65f / books.Length);
			
			foreach(var book in books)
			{
				var child = booksParent.GetChild(book).gameObject;
					child.SetActive(true);
				
				yield return step;
			}
		}
	}
}