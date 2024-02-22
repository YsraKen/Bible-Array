using UnityEngine;

public class ToggleLeftAttribute : PropertyAttribute
{
	/* 
		Description:
			Makes a toggle field where the toggle is to the left and the label immediately to the right of it.
		
		How to use:
			Apply the ToggleLeft attribute to a Boolean field.
		
		Example:
			[ToggleLeft] public bool myBool;
			
			void Start()
			{
				Debug.Log(myBool);
			}
	*/	
}