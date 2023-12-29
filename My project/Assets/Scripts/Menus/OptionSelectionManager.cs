using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionSelectionManager : MonoBehaviour
{
    public Button[] buttons;
    private int selectedButton;

	private void Start()
	{
        if(buttons.Length == 0)
		{
            Debug.Log("No has seleccionado los botones");
		}

        selectedButton = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            int index = i;
            button.onClick.AddListener(() => SelectButton(index));
        }
    }

    private void HighlightButton()
	{
        for(int i = 0; i < buttons.Length; i++)
		{
            Button button = buttons[i];
            if(i == selectedButton)
			{
                button.interactable = false;
            } else
			{
                button.interactable = true;
			}
            
		}
	}

    public void SelectButton(int idx)
    {
        selectedButton = idx;
	}
   
	void Update()
    {
        HighlightButton();
    }
}
