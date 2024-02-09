using UnityEngine;
using UnityEditor;

public class PhoneGameWindow : EditorWindow
{
	[MenuItem("Budog/Phone - Game Window")]
	public static void ShowPhoneGameWindow()
	{
		var window = GetWindow<PhoneGameWindow>("Budog Realme Phone");
		
		float normalizedValue = 0.375f;
		
		var size = Vector2.one * (1560f * normalizedValue);
			
			window.minSize = size;
			window.maxSize = size;
	}
	
	void OnGUI() => GUILayout.Label("Please drag and drop the game window here to simulate budog's realme phone", EditorStyles.centeredGreyMiniLabel);
}