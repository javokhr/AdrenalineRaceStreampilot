using UnityEngine;
using System.Collections.Generic;
using SickscoreGames.HUDNavigationSystem;
using FGear;

public class CarSpawner : MonoBehaviour
{
    public static GameObject SpawnedCar { get; private set; }
    [SerializeField] private List<GameObject> carPrefabs;
    private GameObject spawnedCar;
    [SerializeField]
    private void Awake()
    {
        SpawnSelectedCar();
    }

    private void SpawnSelectedCar()
    {
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex");
        if (selectedCarIndex >= 0 && selectedCarIndex < carPrefabs.Count)
        {
            spawnedCar = Instantiate(carPrefabs[selectedCarIndex], transform.position, transform.rotation);
            OrbitCamera orbitCamera = FindObjectOfType<OrbitCamera>();
            HUDNavigationSystem hUDNavigationSystem = FindObjectOfType<HUDNavigationSystem>();

            if (orbitCamera != null)
            {
                orbitCamera.setTarget(spawnedCar.transform);
            }

            if (hUDNavigationSystem != null)
            {
                hUDNavigationSystem.SetCarTransform(spawnedCar.transform); // Устанавливаем transform в OtherClass
            }

            // Замораживаем вашу машину
            FreezeCar(spawnedCar);
            // Вызываем разморозку через 3 секунды
            Invoke("UnfreezeCar", 3f);
        }
        else
        {
            Debug.LogWarning("Неверный индекс выбранного автомобиля!");
        }
    }

    public void FreezeCar(GameObject car)
    {
        Vehicle carVehicle = car.GetComponent<Vehicle>();

        // Проверяем, что у нас есть компонент Vehicle
        if (carVehicle != null)
        {
            // Здесь вам нужно выполнить логику для заморозки вашей машины
            // Ниже представлен пример, но это может потребовать специфической логики в зависимости от ваших компонентов и требований

            // Выключаем управление
            carVehicle.getStandardInput().setEnabled(false);

            // Сбрасываем ввод
            carVehicle.getStandardInput().resetInputs();

            // Отключаем статистику (если есть)
            if (carVehicle.GetComponent<Statistics>() != null)
            {
                carVehicle.GetComponent<Statistics>().enabled = false;
            }

            // Отключаем отображение индикаторов (если есть)
            if (carVehicle.GetComponent<GaugeUI>() != null)
            {
                carVehicle.GetComponent<GaugeUI>().enabled = false;
            }

            // Отключаем отладочные компоненты (если есть)
            if (carVehicle.GetComponent<MyDebug>() != null)
            {
                carVehicle.GetComponent<MyDebug>().enabled = false;
            }
            /*
            // Устанавливаем громкость эффектов в 0 (или другое значение, в зависимости от вашей логики)
            if (carVehicle.GetComponent<Effects>() != null)
            {
                carVehicle.GetComponent<Effects>().setVolume(0f);
            }
            */
        }
        else
        {
            Debug.LogError("Ошибка: компонент Vehicle не найден на машине.");
        }

        // Ваш код для фокусировки камеры на вашей машине
        // Пример использования OrbitCamera, подобный CarManager
        OrbitCamera orbitCamera = FindObjectOfType<OrbitCamera>();
        if (orbitCamera != null)
        {
            orbitCamera.setTarget(car.transform);
        }

        // Ваш код для изменения цвета мини-карты
        GameObject miniQuad = Utility.findChild(car, "minimapQuad");
        if (miniQuad != null)
        {
            miniQuad.GetComponent<Renderer>().material.color = Color.red;
            miniQuad.transform.localPosition = 20f * Vector3.up;
        }

        // Вызываем разморозку через 3 секунды
        Invoke("UnfreezeCar", 3f);
    }

    public void UnfreezeCar()
    {
        // Здесь вам нужно выполнить логику для разморозки вашей машины
        // Это может включать в себя возврат всех настроек, отключение режима мута и так далее.
        // Вы можете использовать аналогичные методы из CarManager, чтобы вернуть машину в нормальное состояние.
        // Например:

        // Включаем управление (или любую другую логику для разморозки)
        Vehicle carVehicle = spawnedCar.GetComponent<Vehicle>();
        if (carVehicle != null)
        {
            carVehicle.getStandardInput().setEnabled(true);
        }

        // Отключаем статистику (если есть)
        if (carVehicle.GetComponent<Statistics>() != null)
        {
            carVehicle.GetComponent<Statistics>().enabled = true;
        }

        // Отключаем отображение индикаторов (если есть)
        if (carVehicle.GetComponent<GaugeUI>() != null)
        {
            carVehicle.GetComponent<GaugeUI>().enabled = true;
        }

        // Отключаем отладочные компоненты (если есть)
        if (carVehicle.GetComponent<MyDebug>() != null)
        {
            carVehicle.GetComponent<MyDebug>().enabled = true;
        }

        // Дополнительные действия при разморозке (если есть)
    }


}
