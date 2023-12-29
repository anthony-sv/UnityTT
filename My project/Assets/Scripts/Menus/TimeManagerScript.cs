using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManagerScript : MonoBehaviour
{
	private float normalTime = 0.5f;
	public bool isPaused = false;

    public void SlowDownTime(float scale)
	{
		Time.timeScale = scale * normalTime;
	}

	public void PauseTime()
	{
		isPaused = true;
		Time.timeScale = 0.0f;
	}

	public void ResumeTime()
	{
		isPaused = false;
		Time.timeScale = normalTime;
	}
}
