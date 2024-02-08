using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BibleDownloaderIterator : MonoBehaviour
{
	public BibleDownloader[] downloaders;
	
	[Space]
	public Text timerTxt;
	public Text bookTimerTxt, versionTimerTxt;
	
	public Image currentBookProgressImg, currentVersionProgressImg, totalProgressImg;
	public float progressSpeedSmooth = 5f;
	
	public Text currentBookProgressTxt, currentVersionProgressTxt, totalProgressTxt;
	
	void Awake()
	{
		Array.ForEach(downloaders, d => d.gameObject.SetActive(false));
	}
	
	IEnumerator Start()
	{
		float startTime = Time.time;
		float timeElapsed = 0f;
		
		BibleDownloader.longestTime = 0f;
		
		foreach(var downloader in downloaders)
		{
			downloader.gameObject.SetActive(true);
			
			while(!downloader.IsDone)
			{
				yield return null;
				
				#region Time trackers
				
				timeElapsed = Time.time - startTime;
				timerTxt.text = $"{TimeSpan.FromSeconds(Mathf.Round(timeElapsed)).ToString()}";
				
				bookTimerTxt.text = $"{TimeSpan.FromSeconds(Mathf.Round(Time.time - downloader.BookStartTime)).ToString()}";
				versionTimerTxt.text = $"{TimeSpan.FromSeconds(Mathf.Round(Time.time - downloader.StartTime)).ToString()}";
				
				#endregion
				
				#region Progress Bars
				
				currentBookProgressImg.fillAmount = downloader.bookProgress;
				currentVersionProgressImg.fillAmount = downloader.allProgress;
				
				float allProgressSum = 0;
				
				Array.ForEach(downloaders, d => allProgressSum += d.allProgress);
				totalProgressImg.fillAmount = allProgressSum / downloaders.Length;
				
				#endregion
				
				#region Progress Texts
				
				currentBookProgressTxt.text = $"<b>{downloader.currentBook.nickname.ToUpper()}:</b> {downloader.currentChapter} | {GetPercent(currentBookProgressImg.fillAmount)}%";
				currentVersionProgressTxt.text = $"<b>{downloader.currentBook.version.NameCode.ToUpper()}:</b> {GetPercent(currentVersionProgressImg.fillAmount)}%";
				totalProgressTxt.text = $"<b>{GetPercent(totalProgressImg.fillAmount)}%</b>";
				
				#endregion
			}
			yield return null;
		}
		
		Debug.Log($"<b>FINISHED!</b> total time: {TimeSpan.FromSeconds(Mathf.Round(timeElapsed)).ToString()}", this);
	}
	
	string GetPercent(float normalizedValue) => (normalizedValue * 100).ToString("0.00");
}