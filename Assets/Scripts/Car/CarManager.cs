// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;
using FGear;
using UnityEngine.UI;

using Lean.Localization;
public class CarManager : MonoBehaviour
{
    public Text countdownText;  // Поле для вашего текстового объекта
    [SerializeField] LeanLocalization _local;
    public GUIStyle countdownStyle;
    [Serializable]
    class PlayerVehicle
    {
        public string PlayerName = "Player";
        public Vehicle Vehicle;
        [NonSerialized]
        public float Progress = 0f;
        [NonSerialized]
        public float LastProgress = 0f;
    }

    [SerializeField]
    bool ShowGUI = true;
    [SerializeField]
    bool ShowRankings = false;
    [SerializeField]
    PlayerVehicle[] VehicleList;
    [SerializeField]
    int StartIndex = 0;
    [SerializeField]
    bool MuteOthers = false;
    [SerializeField]
    OrbitCamera Camera;
    [SerializeField]
    SplineTool Spline;
    [SerializeField]
    float CountDown = 0f;

    Rect mWindowRect = new Rect(Screen.width - 260, 5, 125, 115);
    int mWinID;
    int mCurrentIndex = 0;
    float mFreezeTime = 0f;
    float mSplineMaxParam = 0f;
    [SerializeField]
    private Vehicle spawnedCar;
    Vehicle currentActiveVehicle;
    bool isCountdownActive = true;

    [SerializeField]
    private FinishPopupManager finishPopupManager;
    // Сохраните индекс финишировавшей машины
    private int finishedCarIndex = -1;

    void Start()
    {
        mWinID = Utility.winIDs++;

        mCurrentIndex = StartIndex;
        if (mCurrentIndex >= VehicleList.Length) mCurrentIndex = 0;

        // Заспавненная машина должна быть первой в списке
        if (CarSpawner.SpawnedCar != null)
        {
            for (int i = 0; i < VehicleList.Length; i++)
            {
                if (VehicleList[i].Vehicle == CarSpawner.SpawnedCar.GetComponent<Vehicle>())
                {
                    mCurrentIndex = i;

                    // Перемещаем заспавненную машину в начало списка
                    MoveVehicleToBeginning(i);

                    break;
                }
            }
        }

        //activate current
        setCurrentActive();

        //count down at start?
        mFreezeTime = CountDown;
        if (mFreezeTime > 0f) setAllFrozen(true);

        //if spline exists, calc. initial vehicle progress/rankings
        if (VehicleList.Length > 0 && Spline != null)
        {
            mSplineMaxParam = Spline.getPath().GetMaxParam();

            for (int i = 0; i < VehicleList.Length; i++)
            {
                float currentPrm = Spline.getClosestParam(VehicleList[i].Vehicle.getPosition());
                VehicleList[i].Progress = currentPrm;
                VehicleList[i].LastProgress = currentPrm;
            }
        }

        /*
    
    if (_local.CurrentLanguage == "Japanese")
    {
        countdownText.text = "ゴー！";
    }

    if (_local.CurrentLanguage == "Chineese")
    {
        countdownText.text = "去！";
    }
    if (_local.CurrentLanguage == "Korean")
    {
        countdownText.text = "GO!";
    }
*/
        // Проверяем, что есть заспавненная машина
        if (CarSpawner.SpawnedCar != null)
        {
            // Находим индекс машины в массиве
            int index = Array.FindIndex(VehicleList, item => item.Vehicle == CarSpawner.SpawnedCar.GetComponent<Vehicle>());

            // Проверяем, что индекс найден
            if (index >= 0)
            {
                mCurrentIndex = index;
                // Ваш код здесь
            }
            else
            {
                Debug.LogError("Машина не найдена в массиве VehicleList.");
            }
        }

        if (_local.CurrentLanguage == "English")
        {
            countdownText.text = "GO!";
        }
        if (_local.CurrentLanguage == "Russian")
        {
            countdownText.text = "ПОЕХАЛИ!";
        }
        if (_local.CurrentLanguage == "Turkish")
        {
            countdownText.text = "GIDELIM!";
        }
        if (_local.CurrentLanguage == "Uzbek")
        {
            countdownText.text = "KETDIK!";
        }
        if (_local.CurrentLanguage == "German")
        {
            countdownText.text = "LOS!";
        }
        if (_local.CurrentLanguage == "Spanish")
        {
            countdownText.text = "¡ADELANTE!";
        }

    }

    void MoveVehicleToBeginning(int index)
    {
        // Перемещаем машину в начало массива
        PlayerVehicle temp = VehicleList[index];
        for (int i = index; i > 0; i--)
        {
            VehicleList[i] = VehicleList[i - 1];
        }
        VehicleList[0] = temp;
    }

    void Update()
    {
        //still frozen?
        if (mFreezeTime > 0f)
        {
            mFreezeTime -= Time.deltaTime;
            if (mFreezeTime <= 0f)
            {
                setAllFrozen(false);
            }
        }
        else if (ShowRankings) updateProgress();

        //reset vehicle with R key
        if (mCurrentIndex < VehicleList.Length)
        {
            Vehicle v = VehicleList[mCurrentIndex].Vehicle;

            if (Input.GetKeyDown(KeyCode.R))
            {
                // Проверяем, что машина, которую мы собираемся перевернуть, действительно существует
                if (v != null)
                {
                    // Проверяем, перевернута ли машина
                    if (isVehicleUpsideDown(v))
                    {
                        // Если машина перевернута, переворачиваем ее
                        Vector3 pos = v.getPosition();
                        Quaternion rot = v.getRotation();
                        rot.eulerAngles = new Vector3(0, rot.eulerAngles.y, 0);

                        // Если spline найден, устанавливаем позицию/вращение в соответствии с текущим параметром
                        if (Spline != null)
                        {
                            float param = Spline.getClosestParam(pos);
                            pos = Spline.getPoint(param);
                            Vector3 target = Spline.getPoint(param + 0.1f);
                            rot.SetLookRotation(target - pos);
                        }

                        // Ваш текущий код для сброса машины
                        v.reset(pos, rot);
                    }
                    else
                    {
                        Debug.LogWarning("Машина не перевернута для индекса " + mCurrentIndex);
                    }
                }
                else
                {
                    Debug.LogError("Vehicle is null for index " + mCurrentIndex);
                }
            }
        }
        if (finishPopupManager != null && finishPopupManager.IsRaceFinished())
        {
            // Гонка завершена, останавливаем ботов
            StopCurrentBot();
            Debug.Log("Finish bots");
        }
        if (finishPopupManager != null && finishPopupManager.IsRaceFinished())
        {
            // Гонка завершена, остановите текущую машину
            StopCurrentVehicle();
            Debug.Log("Finish");
        }
    }

    bool isVehicleUpsideDown(Vehicle vehicle)
    {
        // Проверяем, перевернута ли машина
        float angle = Vector3.Angle(Vector3.up, vehicle.transform.up);
        return angle > 90f;
    }
    void StopCurrentVehicle()
    {
        if (mCurrentIndex < VehicleList.Length)
        {
            Vehicle v = VehicleList[mCurrentIndex].Vehicle;
            AIController ai = v.GetComponent<AIController>();

            if (ai != null)
            {
                // Остановите только ботов
                ai.enabled = false;
                v.getStandardInput().setStartGridMode(true);
            }
            else
            {
                Debug.LogError("Не удалось получить AIController для текущей машины.");
            }
        }
    }

    void StopCurrentBot()
    {
        if (mCurrentIndex < VehicleList.Length)
        {
            Vehicle v = VehicleList[mCurrentIndex].Vehicle;
            AIController ai = v.GetComponent<AIController>();

            if (ai != null && ai.enabled)
            {
                // Гонка завершена, остановить бота
                ai.enabled = false;
                v.getStandardInput().setStartGridMode(true);
            }
        }
    }
    void setVehicleActive(int index, bool active)
    {
        //activate/deactivate components acc. to active state
        Vehicle v = VehicleList[index].Vehicle;
        AIController ai = v.GetComponent<AIController>();
        bool isAI = ai != null && ai.enabled && ai.isActive();
        v.getStandardInput().setEnabled(active && !isAI);
        v.getStandardInput().resetInputs();
        if (v.GetComponent<Statistics>() != null) v.GetComponent<Statistics>().enabled = active;
        if (v.GetComponent<GaugeUI>() != null) v.GetComponent<GaugeUI>().enabled = active;
        if (v.GetComponent<MyDebug>() != null) v.GetComponent<MyDebug>().enabled = active;
        if (v.GetComponent<Effects>() != null) v.GetComponent<Effects>().setVolume((active || !MuteOthers) ? 1f : 0f);

        //focus camera on active vehicle
        if (active) Camera.setTarget(v.transform);

        //active vehicles minimap quad has different color
        GameObject miniQuad = Utility.findChild(v.gameObject, "minimapQuad");
        if (miniQuad != null)
        {
            miniQuad.GetComponent<Renderer>().material.color = active ? Color.red : Color.blue;
            miniQuad.transform.localPosition = (active ? 20f : 10f) * Vector3.up;
        }
    }

    void setCurrentActive()
    {
        // Отключаем все машины
        for (int i = 0; i < VehicleList.Length; i++)
        {
            setVehicleActive(i, false);
        }

        // Если у нас есть активная машина, включаем её
        if (currentActiveVehicle != null)
        {
            int index = Array.FindIndex(VehicleList, item => item.Vehicle == currentActiveVehicle);
            if (index >= 0)
            {
                setVehicleActive(index, true);
            }
            else
            {
                Debug.LogError("Ошибка: Текущая активная машина не найдена в списке!");
            }
        }
    }

    //called at start grid
    void setAllFrozen(bool freeze)
    {
        for (int i = 0; i < VehicleList.Length; i++)
        {
            Vehicle v = VehicleList[i].Vehicle;
            v.getStandardInput().setStartGridMode(freeze);
            v.setBraking(1f, false);
            AIController ai = v.GetComponent<AIController>();
            if (ai != null) ai.enabled = !freeze;
        }
    }

    void updateProgress()
    {
        if (Spline != null)
        {
            // Обновляем прогресс машин на сплайне
            for (int i = 0; i < VehicleList.Length; i++)
            {
                Vehicle v = VehicleList[i].Vehicle;
                AIController ai = v.GetComponent<AIController>();

                float currentPrm = 0f;

                // для AI прогресс уже найден, считываем из компонента ai
                if (ai != null && ai.isActive())
                {
                    currentPrm = ai.getCurrentSplineParam();
                    if (currentPrm < 0f) continue;
                }
                // для управляемых игроком машин, находим текущий параметр
                else currentPrm = Spline.getClosestParam(VehicleList[i].Vehicle.getPosition());

                // как много изменений с прошлого кадра
                float lastPrm = VehicleList[i].LastProgress;
                VehicleList[i].LastProgress = currentPrm;
                float progress = currentPrm - lastPrm;

                // проверяем проход через финишную линию
                if (progress < -0.5f * mSplineMaxParam) progress = mSplineMaxParam + progress;

                // добавляем
                VehicleList[i].Progress += progress;
                if (VehicleList[i].Progress >= mSplineMaxParam)
                {
                    Vehicle vehicle = VehicleList[i].Vehicle;

                    // Добавьте отладочное сообщение, чтобы увидеть, что мы пытаемся остановить машину
                    Debug.Log("Пытаемся остановить машину: " + vehicle.name);

                    // Останавливаем двигатель и устанавливаем тормоз
                    vehicle.getEngine().setThrottle(0f);
                    vehicle.setBraking(1f, false);

                    // Добавьте отладочное сообщение, чтобы увидеть, что машина действительно остановилась
                    Debug.Log("Машина остановлена: " + vehicle.name);
                }

            }

            // должны быть отсортированы в соответствии с прогрессом машины/ранжированием
            Vehicle cVehicle = VehicleList[mCurrentIndex].Vehicle;
            Array.Sort(VehicleList, delegate (PlayerVehicle p1, PlayerVehicle p2) { return p2.Progress.CompareTo(p1.Progress); });

            // находим правильный индекс текущей машины
            for (int i = 0; i < VehicleList.Length; i++)
            {
                if (cVehicle == VehicleList[i].Vehicle)
                {
                    mCurrentIndex = i;
                    break;
                }
            }
        }

    }


    void OnGUI()
    {
        if (mFreezeTime > 0f)
        {
            countdownStyle.fontSize = 120;
            countdownStyle.alignment = TextAnchor.MiddleCenter;
            float x = 0.5f * Screen.width;
            GUI.Label(new Rect(x - 175, 0f, 350, 300f), mFreezeTime > 0.5f ? mFreezeTime.ToString("f0") : countdownText.text, countdownStyle);
            countdownStyle.fontSize = 12;
            countdownStyle.alignment = TextAnchor.UpperLeft;
        }
        else
        {
            if (ShowGUI)
            {
                mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Car Select");
            }

            if (ShowRankings)
            {
                GUI.skin.label.fontSize = 15;
                for (int i = 0; i < VehicleList.Length; i++)
                {
                    GUI.contentColor = i == mCurrentIndex ? Color.red : Color.white;
                    GUI.Label(new Rect(Screen.width - 125f, 20f + i * 30f, 200f, 30f), (i + 1).ToString().PadRight(3) + VehicleList[i].PlayerName);
                }
                GUI.skin.label.fontSize = 12;
                GUI.contentColor = Color.white;
            }
        }
    }

    void uiWindowFunction(int windowID)
    {
        if (GUI.Button(new Rect(10, 25, 105, 25), "Next Car"))
        {
            mCurrentIndex++;
            mCurrentIndex %= VehicleList.Length;
            setCurrentActive();
        }

        if (GUI.Button(new Rect(10, 55, 105, 25), "Toggle Engine"))
        {
            bool running = VehicleList[mCurrentIndex].Vehicle.getEngine().isRunning();
            VehicleList[mCurrentIndex].Vehicle.getEngine().setEngineRunning(!running);
        }

        AIController ai = VehicleList[mCurrentIndex].Vehicle.GetComponent<AIController>();
        bool isAI = ai != null && ai.enabled && ai.isActive();
        isAI = GUI.Toggle(new Rect(10, 85, 105, 20), isAI, isAI ? " AI Enabled" : "AI Disabled");
        VehicleList[mCurrentIndex].Vehicle.getStandardInput().setEnabled(!isAI);
        if (ai != null) ai.setActive(isAI);
    }
}