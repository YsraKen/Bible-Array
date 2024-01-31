using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Tag
{
	public string value;
	public int index;
	public int closingIndex;
}

[System.Serializable]
public class HtmlTagIdentifier
{
	public List<Tag> tags = new List<Tag>();
	
	public string openingTag = "<span";
	
	[TextArea(10,10)]
	public string html;

	public void IdentifySpanTags()
	{
		Stack<Tag> tagStack = new Stack<Tag>();
		
		for (int i = 0; i < html.Length; i++)
		{
			if(i + (openingTag.Length-1) < html.Length)
			{
				string substring = html.Substring(i, openingTag.Length);

				if (substring.Equals(openingTag, StringComparison.OrdinalIgnoreCase))
				{
					int closingIndex = FindClosingTagIndex(html, i);
					
					if (closingIndex != -1)
					{
						string tagValue = html.Substring(i, closingIndex - i + 7);

						Tag tag = new Tag
						{
							value = tagValue,
							index = i,
							closingIndex = closingIndex
						};

						tags.Add(tag);

						// If it's not a self-closing tag, push it onto the stack
						if (!tagValue.Contains("/>"))
						{
							tagStack.Push(tag);
						}

						i = closingIndex + 6; // Skip the processed part
					}
				}
			}
		}
	}

	private int FindClosingTagIndex(string html, int startIndex)
	{
		int endIndex = html.IndexOf('>', startIndex);
		
		if (endIndex != -1)
		{
			// Check if it's a self-closing tag
			if (html[endIndex - 1] == '/')
			{
				return endIndex;
			}

			string closingTag = $"</span>";

			return html.IndexOf(closingTag, endIndex);
		}

		return -1; // Closing tag not found
	}

	public static void Main(ref HtmlTagIdentifier tagIdentifier)
	{
		tagIdentifier = new HtmlTagIdentifier();
		tagIdentifier.html = @"<span>
	<span>
		<span>
			<span>
				Test
			</span>
		</span>
	</span>
</span>";

		tagIdentifier.IdentifySpanTags();
		Debug.Log(tagIdentifier.tags.Count);
		
		// Access identified span tags
		foreach (var tag in tagIdentifier.tags)
		{
			Debug.Log($"Tag: {tag.value}, Opening Index: {tag.index}, Closing Index: {tag.closingIndex}");
		}
	}
}
