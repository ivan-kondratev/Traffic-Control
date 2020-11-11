using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CarController : MonoBehaviour,IPointerClickHandler
{
    private CarMovement carMovement;

    private MeshRenderer speedTextRenderer;
    private TextMesh speedTextMesh;

    private PlayerResources playerResources;
    private LosingController losingController;
    private bool resourcesSetted;

    private float speedTextOffPoint;
    private float checkResourcesPoint;

    private const string PlayerTag = "Player";
    private const string LosingControllerTag = "LosingController";

    //Finds car's movement script ...
    private void Start()
    {
        carMovement = GetComponentInParent<CarMovement>();
        playerResources = GameObject.FindWithTag(PlayerTag).GetComponent<PlayerResources>();
        losingController = GameObject.FindWithTag(LosingControllerTag).GetComponent<LosingController>();

        speedTextOffPoint = carMovement.Road.commonPath.GetControlPoint(15).z;
        checkResourcesPoint = carMovement.Road.commonPath.GetControlPoint(47).z;

        SetTextParameters();
    }

    private void FixedUpdate()
    {
        TextShutdownCheck();
        if (!resourcesSetted)
        {
            SetPlayerResorces();
        }

    }

    //Changes car's common path if it hasn't changed yet to the closest police path 
    private void ChangePath()
    {
        if (carMovement.CurrentPath.GetType().Equals(typeof(CommonPath)))
        {
            int index = FirstAvailablePolicePathIndex();
            if (index == -1) return;
            carMovement.CurrentPath = carMovement.Road.policePaths[index];
            carMovement.Road.policePaths[index].Occupied = true;
        }
    }

    //Returns the index of the first available police way if car not crossed police way's change point
    private int FirstAvailablePolicePathIndex()
    {
        for (int i = 0; i < carMovement.Road.policePaths.Length; i++)
            if (carMovement.transform.position.z <= carMovement.Road.policePaths[i].ChangePoint.z)
                if (!carMovement.Road.policePaths[i].Occupied) return i;
        return -1;
    }

    //Detects click on car
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -1)
            ChangePath();
    }
    
    private void TextShutdownCheck()
    {
        if (carMovement.transform.position.z > speedTextOffPoint)
            speedTextRenderer.enabled = false;
    }
    
    private void SetTextParameters()
    {
        speedTextRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        speedTextMesh = transform.GetChild(0).GetComponent<TextMesh>();
        speedTextMesh.text = Math.Round(carMovement.MoveSpeedInSpeedUnit, 1) + " km/h";
    }

    private void SetPlayerResorces()
    {
        if (carMovement.CurrentPath.GetType().Equals(typeof(CommonPath)))
        {
            if (carMovement.transform.position.z > checkResourcesPoint)
            {
                if (carMovement.MoveSpeedInSpeedUnit > SpeedLimit.SpeedLimitValue + Speeding.SpeedingValue)
                {
                    PlayerResources.health--;
                    playerResources.hearts[playerResources.LastHeartIndex].SetActive(false);
                    playerResources.LastHeartIndex--;
                    if (losingController.PlayerLose())
                    {
                        losingController.ActivateLosingPanel();
                    }
                }
                else
                {
                    PlayerResources.score++;
                    playerResources.scoreText.text = PlayerResources.score.ToString();
                }
                resourcesSetted = true;
            }
        }
        else
        {
            if (carMovement.Stopped)
            {
                if (carMovement.MoveSpeedInSpeedUnit > SpeedLimit.SpeedLimitValue)
                {
                    PlayerResources.score++;
                    playerResources.scoreText.text = PlayerResources.score.ToString();

                }
                else
                {
                    PlayerResources.health--;
                    playerResources.hearts[playerResources.LastHeartIndex].SetActive(false);
                    playerResources.LastHeartIndex--;
                    if (losingController.PlayerLose())
                    {
                        losingController.ActivateLosingPanel();
                    }
                }
                resourcesSetted = true;
            }
        }
    }
}
