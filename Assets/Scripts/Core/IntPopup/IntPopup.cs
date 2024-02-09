using System.Collections.Generic;

[System.Serializable]
public class IntPopup
{
	public string name;
	public int value;
	
	#if UNITY_EDITOR
	public string[] options;
	#endif
	
	public bool showInt;
	
	#region Constructors
		
		public IntPopup(int value) =>
			this.value = value;
		
		#if UNITY_EDITOR
		public IntPopup(params string[] options)
		{
			this.options = options;
		}
			
		
		public IntPopup(int value, params string[] options)
		{
			this.value = value;
			
			this.options = options;
		}
		
		public void SetOptions(params string[] options)
		{
			this.options = options;
		}
		#endif
		
	#endregion
	
	#region Conversions
		
		public static implicit operator int(IntPopup ip) => ip.value;
		public static implicit operator IntPopup(int value) => new IntPopup(value);
		
	#endregion
	
	public T GetElement<T>(T[] array) => array[value];
	public T GetElement<T>(List<T> list) => list[value];
	
	public virtual void OnEditorLoad(){}
	public virtual void OnEditorUpdate(){}
}