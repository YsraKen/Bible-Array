using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class MarkManager : ScriptableObject
{
	[field: SerializeField]
	public List<MarkInfo> MarkInfos { get; private set; } = new List<MarkInfo>();
	
	public int GetDatabaseIndex(string category, HighlightInfo.Mark value, Version version, int verseIndex)
	{
		int index = 0;
		
		if(!MarkInfos.TryFindIndex(m => m.category == category && m.value.Compare(value), out index))
		{
			index = MarkInfos.Count;
			MarkInfos.Add(new MarkInfo(category, value));
		}
		
		var mgr = GameManager.Instance;
		
		var indicies = new int[]
		{
			version.Language.GetIndex(),
			version.GetIndex(),
			mgr.CurrentBookIndex,
			mgr.CurrentChapterIndex,
			verseIndex
		};
		
		var targets = MarkInfos[index].appliedTo;
		
		bool exists = targets.IsNullOrEmpty()? false:
			Array.Exists(targets, target => target.Compare(indicies));
		
		if(!exists)
		{
			var bibleTarget = new BibleTarget(indicies);
			string bookName = ((BookSelect) version.Books[mgr.CurrentBookIndex]).Info(mgr.GeneralInfo).name;
			
			bibleTarget.name = $"{version.Language.NameCode}-{version.NameCode}-{bookName}[{mgr.CurrentChapterIndex + 1}:{verseIndex + 1}]";
			
			Tools.AddToArray(ref MarkInfos[index].appliedTo, bibleTarget);
		}
		return index;
	}
	
	public void RemoveOn(int index, params int[] bibleTarget)
	{
		if(!MarkInfos.IsInsideRange(index))
			return;
		
		bool exists = MarkInfos[index].appliedTo.TryFindIndex(target => target.Compare(bibleTarget), out int targetIndex);
		
		if(exists)
		{
			Tools.RemoveToArray(ref MarkInfos[index].appliedTo, targetIndex);
			
			// if(MarkInfos[index].appliedTo.IsNullOrEmpty())
				// MarkInfos.RemoveAt(index);
		}
	}
	
	public void LoadData()
	{
		if(SaveManager.TryLoad<UserData>("MarkManager", out var userData))
			MarkInfos = userData.markInfos.ToList();
	}
	
	public void SaveData()
	{
		var userData = new UserData(){ markInfos = MarkInfos.ToArray() };
		SaveManager.Save(userData, "MarkManager");
	}
	
	[Serializable]
	public class MarkInfo
	{
		public string category;
		public HighlightInfo.Mark value;
		public BibleTarget[] appliedTo;
		
		public MarkInfo(string category, HighlightInfo.Mark value)
		{
			this.category = category;
			this.value = value;
		}
	}
	
	[Serializable]
	public struct BibleTarget
	{
		[HideInInspector]
		public string name;
		
		public int language;
		public int version;
		public int book;
		public int chapter;
		public int verse;
		
		public BibleTarget(params int[] indicies)
		{
			name = "";
			
			language = indicies[0];
			version = indicies[1];
			book = indicies[2];
			chapter = indicies[3];
			verse = indicies[4];
		}
		
		public bool Compare(params int[] indicies)
		{
			if(indicies.IsNullOrEmpty())
				return false;
			
			if(indicies.Length != 5)
				return false;
			
			return
				indicies[0] == language &&
				indicies[1] == version &&
				indicies[2] == book &&
				indicies[3] == chapter &&
				indicies[4] == verse;
		}
	}
	
	[Serializable]
	public class UserData
	{
		public MarkInfo[] markInfos;
	}
}