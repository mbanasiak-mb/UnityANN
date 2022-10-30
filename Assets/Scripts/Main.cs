using ANN;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    public uint carsAmmount;
    public GameObject spawnPoint;

    public GameObject textCars;
    public GameObject textSpeed;
    public GameObject textGeneration;

    public GameObject carPrefab;

    private float timeScale;
    private float timeScaleMax;
    private float timeScaleMin;
    private float timeScaleStep;

    private int generationNumber;

    private GameObject allCars;

    private void Start()
    {
        timeScale = 1f;
        timeScaleMax = 4f;
        timeScaleMin = 1f;
        timeScaleStep = 0.2f;

        SetTextSpeed();
        generationNumber = 1;

        allCars = gameObject;
        SpawnCars(carsAmmount);

        textCars.GetComponent<TextMeshPro>().text = "Cars: " + carsAmmount;
    }

    private void Update()
    {
        UserInput();
    }

    private void UserInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PauseSimulation();
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            SpeedUpSimulation();
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            SpeedDownSimulation();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSimulation();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            NextEpoch();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ClearChoosenCars();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void SpeedUpSimulation()
    {
        timeScale += timeScaleStep;

        if (timeScale > timeScaleMax)
        {
            timeScale = timeScaleMax;
        }

        timeScale = Mathf.Round(timeScale * 10) / 10;
        Time.timeScale = timeScale;
        SetTextSpeed();
    }

    private void SpeedDownSimulation()
    {
        timeScale -= timeScaleStep;

        if (timeScale < timeScaleMin)
        {
            timeScale = timeScaleMin;
        }

        timeScale = Mathf.Round(timeScale * 10) / 10;
        Time.timeScale = timeScale;
        SetTextSpeed();
    }

    private void PauseSimulation()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = timeScale;
        }
        else
        {
            Time.timeScale = 0;
        }

        SetTextSpeed();
    }

    private void ResetSimulation()
    {
        for (int i = 0; i < allCars.transform.childCount; i++)
        {
            Destroy(allCars.transform.GetChild(i).gameObject);
        }

        SpawnCars(carsAmmount);

        if (Time.timeScale == 0)
        {
            PauseSimulation();
        }

        generationNumber = 1;
        SetTextGeneration();
    }

    private void NextEpoch()
    {
        ResetCars();

        if (Time.timeScale == 0)
        {
            PauseSimulation();
        }

        SetTextGeneration();
    }

    private void ClearChoosenCars()
    {
        for (int i = 0; i < allCars.transform.childCount; i++)
        {
            allCars.transform.GetChild(i).gameObject.GetComponent<CarAutomatic>().bestCar = false;

            if (allCars.transform.GetChild(i).gameObject.GetComponent<CarAutomatic>().crashedCar)
            {
                allCars.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                allCars.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            }
        }
    }

    private void SetTextGeneration()
    {
        textGeneration.GetComponent<TextMeshPro>().text = "Generation = " + generationNumber;
    }

    private void SetTextSpeed()
    {
        textSpeed.GetComponent<TextMeshPro>().text = "Speed = " + Time.timeScale;
    }

    private void SetCar(GameObject car)
    {
        car.transform.position = spawnPoint.transform.position;
        car.transform.rotation = new Quaternion(0, 0, 0, 0);

        car.gameObject.GetComponent<CarAutomatic>().bestCar = false;
        car.gameObject.GetComponent<CarAutomatic>().crashedCar = false;
        car.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    private void SpawnCars(uint ammount)
    {
        GameObject newCar;

        for (int i = 0; i < ammount; i++)
        {
            newCar = Instantiate(carPrefab);
            newCar.name = "Car_n" + i;
            newCar.transform.parent = allCars.transform;

            SetCar(newCar);
        }
    }

    private void ResetCars()
    {
        Transform car;

        NeuralNetworkLearn();

        for (int i = 0; i < allCars.transform.childCount; i++)
        {
            car = allCars.transform.GetChild(i);

            SetCar(car.gameObject);
        }
    }

    private void NeuralNetworkLearn()
    {
        List<List<List<double>>> car;
        List<List<List<double>>> currentCar;
        List<List<List<List<double>>>> bestCars = new List<List<List<List<double>>>>();

        for (int i = 0; i < allCars.transform.childCount; i++)
        {
            if (allCars.transform.GetChild(i).gameObject.GetComponent<CarAutomatic>().bestCar)
            {
                car = allCars.transform.GetChild(i).gameObject.GetComponent<CarAutomatic>().network.GetWeights();
                bestCars.Add(car);
                break;
            }
        }

        if(bestCars.Count != 0)
        {
            generationNumber += 1;

            foreach (var bestCar in bestCars)
            {
                for (int i = 0; i < allCars.transform.childCount; i++)
                {
                    currentCar = allCars.transform.GetChild(i).gameObject.GetComponent<CarAutomatic>().network.GetWeights();
                    car = LearnNetwork.MixGenes(bestCar, currentCar);
                    allCars.transform.GetChild(i).gameObject.GetComponent<CarAutomatic>().network.SetWeights(car);
                }
            }
        }
    }
}
