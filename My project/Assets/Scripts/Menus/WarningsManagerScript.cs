using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningsManagerScript : MonoBehaviour
{
    [SerializeField] private GameObject InitialValuesMenu;
    [SerializeField] private GameObject WarningDialog;


    public void EnableWarning()
	{
        InitialValuesMenu.SetActive(false);
        WarningDialog.SetActive(true);
	}

    public void DisableWarning()
	{
        InitialValuesMenu.SetActive(true);
        WarningDialog.SetActive(false);
    }

}
