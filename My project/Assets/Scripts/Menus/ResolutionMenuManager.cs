using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResolutionMenuManager : MonoBehaviour
{
	[SerializeField] private TMP_Dropdown resolutionsDropdown;
	[SerializeField] private Toggle fullScreenToggle;

	private Resolution[] resolutions;

	private int currentResolutionIndex = 0;
	private bool fullScreen;

	private void Start()
	{
		StartResolutions();

		fullScreen = Screen.fullScreen;
		fullScreenToggle.isOn = fullScreen;
	}

	private void StartResolutions()
	{
		resolutions = Screen.resolutions;
		System.Array.Reverse(resolutions);

		List<string> options = new List<string>();
		for (int i = 0; i < resolutions.Length; i++)
		{
			string resolutionOption = resolutions[i].width + "x" + resolutions[i].height + "#" + resolutions[i].refreshRate + "Hz";
			options.Add(resolutionOption);

			if (Screen.width == resolutions[i].width && Screen.height == resolutions[i].height)
			{
				currentResolutionIndex = i;
			}
		}

		resolutionsDropdown.ClearOptions();
		resolutionsDropdown.AddOptions(options);
		resolutionsDropdown.value = currentResolutionIndex;
		resolutionsDropdown.RefreshShownValue();
	}

	public void SetResolution(int idx)
	{
		Screen.SetResolution(resolutions[idx].width, resolutions[idx].height, fullScreen);
	}

	public void ToggleWindowMode()
	{
		fullScreen = fullScreenToggle.isOn;
		Screen.fullScreen = fullScreen;
	}
}
