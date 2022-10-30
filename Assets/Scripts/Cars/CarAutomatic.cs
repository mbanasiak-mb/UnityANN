using ANN;
using UnityEngine;
using System.Collections.Generic;

public class CarAutomatic : MonoBehaviour
{
    public bool bestCar;
    public bool crashedCar;
    public float accelerationPower;
    public float steeringPower;

    public NeuralNetwork network;

    private float distance1;
    private float distance2;
    private float distance3;
    private float carSpeedMagnitude;

    private float verticalAction;
    private float horizontalAction;

    private readonly float steeringAmount;

    private Rigidbody2D rb;
    private LayerMask mapLayer;

    private void Start()
    {
        bestCar = false;
        crashedCar = false;

        rb = GetComponent<Rigidbody2D>();
        mapLayer = LayerMask.GetMask("Map");

        SensorsMeasure();
        CreateNetwork();
    }

    private void FixedUpdate()
    {
        if (crashedCar)
        {
            return;
        }

        SensorsMeasure();
        NetworkDecide();
        MoveCar();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Edit -> ProjectSettings... -> Physics2D -> bottom

        crashedCar = true;
        Crashed();        
    }

    private void OnMouseDown()
    {
        ClickCar();
    }

    private void MoveCar()
    {
        float speed = verticalAction * accelerationPower;
        float direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));

        rb.rotation += -horizontalAction * steeringPower * rb.velocity.magnitude * direction;
        rb.AddRelativeForce(Vector2.up * speed);
        rb.AddRelativeForce(-Vector2.right * rb.velocity.magnitude * steeringAmount / 2);
    }

    private void ClickCar()
    {
        bestCar = !bestCar;

        if (bestCar)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            if (crashedCar)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            }
        }
    }

    private void CreateNetwork()
    {
        network = new NeuralNetwork(4);

        network.AddLayer(5, Functions.LeakyReLU);
        network.AddLayer(5, Functions.LeakyReLU);
        network.AddLayer(4, Functions.Logistic);

        network.InitializeWeights();
    }

    private void SensorsMeasure()
    {
        distance1 = CheckDistance(Vector2.up);
        distance2 = CheckDistance(new Vector2(-0.5f, 1f));
        distance3 = CheckDistance(new Vector2(0.5f, 1f));

        carSpeedMagnitude = rb.velocity.magnitude;
    }

    private void Crashed()
    {
        if (crashedCar)
        {
            if (!bestCar)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    private float CheckDistance(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, transform.rotation * direction, Mathf.Infinity, mapLayer);

        if (hit.transform != null)
        {
            return hit.distance;
        }
        else
        {
            Debug.Log("Raycast HIT - FAIL");
            return -1;
        }
    }

    private void NetworkDecide()
    {
        List<double> input = new List<double>() { distance1, distance2, distance3, carSpeedMagnitude };
        network.SetInput(input);
        network.FeedForward();
        List<double> output = network.GetOutput();

        if (output[0] > output[1])
        {
            horizontalAction = 1f;
        }
        else
        {
            horizontalAction = -1f;
        }

        if (output[2] > output[3])
        {
            verticalAction = 1f;
        }
        else
        {
            verticalAction = -1f;
        }
    }
}
