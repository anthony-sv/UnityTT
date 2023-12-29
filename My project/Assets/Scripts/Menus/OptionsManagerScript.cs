using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsManagerScript : MonoBehaviour
{
    // Initial Value
    [SerializeField] private GameObject InitialValuesMenu;
    [SerializeField] private TMP_InputField VelocityInput;
    [SerializeField] private TMP_InputField AngleInput;
    [SerializeField] private GameObject WallSelection;
    [SerializeField] private GameObject[] walls;
    [SerializeField] private Button PlayButton;

    // Cameras
    [SerializeField] private GameObject CamerasMenu;
    [SerializeField] private GameObject CameraSelection;

    [SerializeField] private Camera[] cameras;

    // Time Menu
    [SerializeField] private GameObject TimeMenu;
    [SerializeField] private GameObject TimeSelection;
    [SerializeField] private Slider TimeSlowerSlide;
    [SerializeField] private Button RestartButton;
    [SerializeField] private GameObject timeManagerObject;
    private TimeManagerScript timeManager;

    // Mass Spring System
    [SerializeField] private GameObject car;
    private CarController controllerCar;

    // Warnings
    [SerializeField] private GameObject warningObject;
    private WarningsManagerScript warningManager;

    void Start()
    {
        controllerCar = car.GetComponent<CarController>();
        warningManager = warningObject.GetComponent<WarningsManagerScript>();

        timeManager = timeManagerObject.GetComponent<TimeManagerScript>();

        SwitchWalls(0);
        WallSelection.GetComponent<OptionSelectionManager>().SelectButton(0);

        SwitchCamera(0);
        CameraSelection.GetComponent<OptionSelectionManager>().SelectButton(0);
    }

    public void Quit()
	{
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    // Update is called once per frame
    void Update()
    {
        SlowDownTime();
    }


    float angle = 0.0f;
    // Initial values
    public void UpdateAngle()
	{
        angle = 0.0f;
		try
		{
            angle = float.Parse(AngleInput.text) % 360;
        }
		catch{}
        controllerCar.SetAngle(angle);
	}

    public void StartSimulation()
	{
        float velocity;
        try
        {
            velocity = 2 * float.Parse(VelocityInput.text);
        }
        catch {
            EnableWarning();
            return;
        }

        bool valuesOK = (-25 <= angle && angle <= 25) && (0 <= velocity && velocity <= 120);
        if (!valuesOK)
		{
            EnableWarning();
            return;
		}

        InitialValuesMenu.SetActive(false);
        controllerCar.SetVelocity(velocity);
	}

    // Walls
    public void SwitchWalls(int idxObstacle)
	{
        DesactivateAllWalls();
        walls[idxObstacle].SetActive(true);
	}
    private void DesactivateAllWalls()
	{
        foreach(GameObject wall in walls)
		{
            wall.SetActive(false);
		}
	}

    // Time
    public void ResumeTime()
	{
        timeManager.ResumeTime();
	}

    public void PauseTime()
	{
        timeManager.PauseTime();
	}

    private void SlowDownTime()
	{
        float scale = TimeSlowerSlide.value;
        if (timeManager.isPaused) return;
        timeManager.SlowDownTime(scale);
	}

    public void ResetSimulation()
	{
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

    // Cameras
    public void SwitchCamera(int idxCamera)
	{
        DesactivateAllCameras();
        cameras[idxCamera].enabled = true;
	}
    private void DesactivateAllCameras()
	{
        foreach(Camera camera in cameras)
		{
            camera.enabled = false;
		}
	}

    // Warning 
    public void DisableWarning()
	{
        warningManager.DisableWarning();
	}
    public void EnableWarning()
	{
        warningManager.EnableWarning();
	}
}
