using UnityEngine;

public class TagSelectAttribute : PropertyAttribute
{
	public bool isTextField{ get; set; }
	
	/* 
		Description:
			Makes a tag selection field.
		
		How to use:
			Apply the TagSelect attribute to a String field.
		
		Example:		
			[TagSelect] public string tag;
			
			void OnTriggerEnter(Collider other)
			{
				if(other.CompareTag(tag))
					Debug.Log(other.name, other);
			}
	*/
}