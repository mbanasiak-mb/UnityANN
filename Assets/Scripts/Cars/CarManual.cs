using UnityEngine;

public class CarManual : MonoBehaviour
{
    public float accelerationPower;
    public float steeringPower;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        MoveCar();
    }

    private void MoveCar()
    {
        float verticalAction = Input.GetAxis("Vertical");
        float horizontalAction = Input.GetAxis("Horizontal");

        float speed = verticalAction * accelerationPower;
        float direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));

        rb.rotation += -horizontalAction * steeringPower * rb.velocity.magnitude * direction;

        rb.AddRelativeForce(Vector2.up * speed);
        rb.AddRelativeForce(-Vector2.right * rb.velocity.magnitude * 1 / 2);
    }
}
