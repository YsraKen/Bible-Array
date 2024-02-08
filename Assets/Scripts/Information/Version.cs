using UnityEngine;

[CreateAssetMenu]
public class Version : ScriptableObject
{
	[field: SerializeField]
	public string Name { get; private set; }
	
	[field: SerializeField]
	public string NameCode { get; private set; }
	
	[field: SerializeField]
	public Language Language { get; private set; }
	
	[field: SerializeField, TextArea]
	public string Description { get; private set; }
	
	[field: SerializeField, Space]
	public string Publisher { get; private set; }
	
	[field: SerializeField, TextArea]
	public string Copyright { get; private set; }
	
	[field: SerializeField]
	public string[] Websites { get; private set; }
	
	[field: SerializeField]
	public Book[] Books { get; private set; }
	
	public void SetBooks(Book[] books) => Books = books;
}