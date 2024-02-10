using UnityEngine;
using UnityEngine.Events;

public class OnDestroyUI : MonoBehaviour
{
	public UnityEvent call;
	
	void OnDestroy() => call.Invoke();
}