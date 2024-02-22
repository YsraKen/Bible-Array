using UnityEngine;

public class SceneSelectAttribute : PropertyAttribute
{
	public bool isTextField{ get; set; }
	
	/* 
		Description:
			Makes a Scene Selection field.
		
		How to use:
			Apply the SceneSelect attribute to an Integer or a String field.
		
		Example:
			[SceneSelect] public int sceneIndex;
			[SceneSelect] public string sceneName;
			
			public void LoadSceneByIndex()
			{
				TalkieDokieSceneManager.Instance.LoadScene(sceneIndex);
			}
			
			public void LoadSceneByName()
			{
				TalkieDokieSceneManager.Instance.LoadScene(sceneName);
			}
		
		Other
			Right click to refresh options
	*/
}