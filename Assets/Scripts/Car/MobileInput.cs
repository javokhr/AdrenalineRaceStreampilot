// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FGear;
using GamePush;
public class MobileInput : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField] private Text SpeedText;

    //inputs
    bool left, right, up, down, handbrake;

    void Start()
    {
        string canvasSpeedName = "Canvas";
        GameObject canvas = GameObject.Find(canvasSpeedName);
        SpeedText = Utility.findChild(canvas, "SpeedText").GetComponent<Text>();
        //найти MobileUICanvas и если он актив то деактивировать
        string canvasName = "MobileUICanvas";
        GameObject mobileUI = GameObject.Find(canvasName);
        if (mobileUI != null)
        {
            mobileUI.SetActive(false);
        }

        // Проверка на мобильную платформу
        //if (Application.platform == RuntimePlatform.WindowsPlayer)
        if (GP_Device.IsMobile())
        {
            //set manual input mode
            Vehicle.getStandardInput().setReadInputs(false);
            //create uicanvas if not found
            mobileUI.SetActive(true);
            Debug.Log("mobileUI = true");
            if (mobileUI != null)
            {
                Button leftButton = Utility.findChild(mobileUI, "leftButton").GetComponent<Button>();
                Button rightButton = Utility.findChild(mobileUI, "rightButton").GetComponent<Button>();
                Button gasButton = Utility.findChild(mobileUI, "gasButton").GetComponent<Button>();
                Button brakeButton = Utility.findChild(mobileUI, "brakeButton").GetComponent<Button>();
                Button handbrakeButton = Utility.findChild(mobileUI, "handbrakeButton").GetComponent<Button>();

                setupButtonInput(leftButton, 0, 1);
                setupButtonInput(rightButton, 2, 3);
                setupButtonInput(gasButton, 4, 5);
                setupButtonInput(brakeButton, 6, 7);
                setupButtonInput(handbrakeButton, 8, 9);
            }
        }

        //SetupEventSystem();
    }

    //assign callbacks to button
    void setupButtonInput(Button b, int actionEnterIndex, int actionExitIndex)
    {
        EventTrigger et = b.gameObject.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((eventData) => { onButton(actionEnterIndex); });
        et.triggers.Add(entry);
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((eventData) => { onButton(actionExitIndex); });
        et.triggers.Add(entry);
    }
    /*
        void SetupEventSystem()
        {
            // Check if EventSystem already exists in the scene
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();

                Debug.Log("EventSystem создано");
            }
        }
    */
    void onButton(int i)
    {
        if (i == 0) left = true;
        else if (i == 1) left = false;
        else if (i == 2) right = true;
        else if (i == 3) right = false;
        else if (i == 4) up = true;
        else if (i == 5) up = false;
        else if (i == 6) down = true;
        else if (i == 7) down = false;
        else if (i == 8) handbrake = true;
        else if (i == 9) handbrake = false;
    }

    void Update()
    {
        float Braking = Vehicle.getAxle(0).getLeftWheel().getBraking();
        //speed text
        if (Braking != 0.0f) SpeedText.text = ((int)Vehicle.getKMHSpeed()).ToString();
        else
        {
            int speed = (int)Vehicle.getMaxWheelKMHSpeed();
            SpeedText.text = speed.ToString();
        }

        if (Vehicle == null) return;
        float gas = up ? 1.0f : 0.0f;
        float brake = down ? 1.0f : 0.0f;
        float steer = left ? -1.0f : (right ? 1.0f : 0.0f);
        Vehicle.getStandardInput().setInputs(gas, brake, steer, 0.0f, 0, handbrake);
    }
}