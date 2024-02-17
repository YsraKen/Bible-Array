using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AppEntry : MonoBehaviour
{
	public bool askForDataClear;
	public bool deleteAllSavedData { get; set; }
	
	[SerializeField] private GameObject _dataClearPopup;
	[SerializeField] private Image _loadProgressImg;
	
	[SerializeField] private GameObject[] _onLoadObjects;
	[SerializeField] private GameObject[] _onActivationObjects;
	
	public static bool IsLoaded { get; private set; }
	
	IEnumerator Start()
	{
		if(askForDataClear)
		{
			Array.ForEach(_onActivationObjects, obj => obj.SetActive(false));
			Array.ForEach(_onLoadObjects, obj => obj.SetActive(false));
			
			yield return null;
			
			_dataClearPopup.SetActive(true);
			yield return new WaitWhile(()=> _dataClearPopup.activeSelf);
			
			if(deleteAllSavedData)
				PlayerPrefs.DeleteAll();
		}
		
		IsLoaded = true;
		
		yield return Exit();
	}
	
	private IEnumerator Exit()
	{
		Array.ForEach(_onActivationObjects, obj => obj.SetActive(false));
		Array.ForEach(_onLoadObjects, obj => obj.SetActive(true));
		
		yield return null;
		
		bool hasSessionData = PlayerPrefs.HasKey(SessionSelect.SAVE_KEY);
		string targetScene = hasSessionData? "SessionSelect": "Gameplay";
		
		var loadOperation = SceneManager.LoadSceneAsync(targetScene);
		
		while(loadOperation.progress < 0.9f)
		{
			_loadProgressImg.fillAmount = Mathf.Clamp01(loadOperation.progress / 0.9f);
			yield return null;
		}
		
		Array.ForEach(_onLoadObjects, obj => obj.SetActive(false));
		Array.ForEach(_onActivationObjects, obj => obj.SetActive(true));
		
		yield return new WaitUntil(()=> loadOperation.isDone);
		yield return null;
	}
}