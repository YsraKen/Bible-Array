using UnityEngine;

public class PasswordAttribute : PropertyAttribute
{
	public bool isRevealed{ get; set; }
	
	/* 
		Description:
			Makes a text field where the user can enter a password.
		
		How to use:
			Apply a Password attribute to a string field.
		
		Example
			[Password] public string password;
		
		Other
			Right click to reveal the string value
	*/
}