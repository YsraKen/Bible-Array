using UnityEngine;

[CreateAssetMenu]
public class Language : ScriptableObject
{
	[field: SerializeField]
	public string Name { get; private set; }
	
	[field: SerializeField]
	public string NameCode { get; private set; }
	
	[field: SerializeField]
	public string Native { get; private set; }
	
	int _index = -1;
	
	public int GetIndex()
	{
		var mgr = GameManager.Instance;
		
		if(_index < 0 && mgr)
			_index = System.Array.IndexOf(mgr.GeneralInfo.allLanguages, this);
		
		return _index;
	}
}