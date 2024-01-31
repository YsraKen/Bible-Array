using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WebPageLoader : MonoBehaviour
{
    public string url = "https://example.com";
	
	[TextArea(20,20)]
	public string sourceCode;
	
    void Start()
    {
        StartCoroutine(LoadWebPage());
    }

    IEnumerator LoadWebPage()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                // Print the HTML source code
                Debug.Log($"HTML Source Code:\n{webRequest.downloadHandler.text}");
				sourceCode = webRequest.downloadHandler.text;
            }
        }
    }
}
