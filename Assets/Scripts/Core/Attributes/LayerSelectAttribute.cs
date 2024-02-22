using UnityEngine;

public class LayerSelectAttribute : PropertyAttribute
{
	/* 
		Description:
			Makes a layer selection field.
		
		How to use:
			Apply the LayerSelect attribute to an int field.
		
		Example:		
			[LayerSelect] public int layer;
			
			void Start()
			{
				gameObject.layer = layer;
			}
	*/
}