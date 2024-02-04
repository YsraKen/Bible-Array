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
}