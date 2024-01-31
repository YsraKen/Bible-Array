using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Test : MonoBehaviour
{
	[TextArea(5,5)]
	public string test;
	
	public List<string> tags = new List<string>();
	
	[ContextMenu("Test")]
	void Test1()
	{
		int index = 0;
		
		while(index < test.Length)
		{
			char c = test[index ++];
			
			if(c == '<')
			{
				int spanIndex = test.IndexOf("<span", index);
				
				if(spanIndex != -1)
				{
					string spanTag = "";
					
					while(c != '>')
					{
						spanTag += c;
						c = test[index ++];
					}
					
					tags.Add(spanTag);
				}
			}
		}
	}
}