using UnityEngine;

public static class SaveManager
{
	// "key" - PlayerPrefs
	// "path" - local file
	// "address" - online
	public static void Save<T>(T data, string key)
	{
		string json = JsonUtility.ToJson(data, true);
		PlayerPrefs.SetString(key, json);
		
		Debug.Log($"Data '<b><color=green>{key}</color></b>' succesfully <b>saved</b>!\n{json}");
	}
	
	public static T Load<T>(string key)
	{
		TryLoad(key, out T data);
		return data;
	}
	
	public static bool TryLoad<T>(string key, out T data)
	{
		data = default(T);
		
		bool hasKey = PlayerPrefs.HasKey(key);
		
		if(hasKey)
		{
			string json = PlayerPrefs.GetString(key);
			data = JsonUtility.FromJson<T>(json);
			
			Debug.Log($"Data '<b><color=cyan>{key}</color></b>' succesfully <b>loaded</b>!\n{json}");
		}
		else
			Debug.LogWarning($"No save data with key '<b><color=yellow>{key}</color></b>' found");
		
		return hasKey;
	}
	
	public static void Delete(string key) => PlayerPrefs.DeleteKey(key);
}