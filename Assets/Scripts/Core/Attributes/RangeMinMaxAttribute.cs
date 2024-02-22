using UnityEngine;

public class RangeMinMaxAttribute : PropertyAttribute
{
	public Vector2 range{ get; }
	
	public RangeMinMaxAttribute(float maxLimit = 1f)
		=> range = new Vector2(0, maxLimit);
		
	public RangeMinMaxAttribute(Vector2 range)
		=> this.range = range;
	
	public RangeMinMaxAttribute(float minLimit, float maxLimit)
		=> range = new Vector2(minLimit, maxLimit);
	
	/* 	
		Description:
			Makes a special slider the user can use to specify a range between a min and a max.
		
		Properties:
			Range - a range where the slider ends are clamped.
		
		Constructors:
			public RangeMinMaxAttribute(float maxLimit)
				
				Parameters:
					maxLimit - The limit at the right end of the slider. The limit of the left end is zero.

			public RangeMinMaxAttribute(Vector2 range);
				
				Parameters:
					range - value to be assigned to the "range" property.
			
			public RangeMinMaxAttribute(float minLimit, float maxLimit)
				
				Parameters:
					minLimit - The limit at the left end of the slider.
					maxLimit - The limit at the right end of the slider.
		
		How to use:
			Apply the RangeMinMax attribute to a Vector2 or Vector2Int field
		
		Example:
			[RangeMinMax(1)] Vector2 minMax;
			[RangeMinMax(10)] Vector2Int minMaxInt;
	*/
}