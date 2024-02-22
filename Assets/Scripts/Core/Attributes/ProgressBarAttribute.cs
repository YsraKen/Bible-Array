using UnityEngine;

public class ProgressBarAttribute : PropertyAttribute
{
	public float max { get; }
	
	public ProgressBarAttribute(float max = 1f) => this.max = max;
	
	/* 	
		Description:
			Makes a progress bar.
		
		How to use:
			Apply the ProgressBar attribute to a Float or an Int field.
		
		Properties:
			max - the max amount where the progress bar considered as full.
		
		Constructors:
			public ProgressBarAttribute(float max = 1f);
			
				Parameters:
					max - the value to be assigned to the property "max".
		
		Example
			[ProgressBar] public int health;
			[ProgressBar] public float healthf;
		
		Other
			Right click to edit the values
	*/
}