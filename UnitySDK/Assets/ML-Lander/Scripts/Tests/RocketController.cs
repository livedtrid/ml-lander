using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RocketController : MonoBehaviour
{
    [System.Flags]
    public enum ROCKET_STATES : int { STEADY = 1 << 0, TILTED = 1 << 1, FAST = 1 << 2, CRASHED = 1 << 3 }
    public ROCKET_STATES rocket_state = ROCKET_STATES.STEADY;

    Rigidbody rb;

    public GameObject leftThrust, centerThrust, rightThrust;
    public float auxiliarTrustForce = 5.0f, centerThrustForce = 10.0f;
    public float safeVelocity = 5.0f; // Meters per second
    public float maxInclination = 15.0f;
    public TMP_Text velocityText;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Reset()
    {
        gameObject.SetActive(true);
        rb.velocity = default(Vector3);
        rb.angularVelocity = default(Vector3);
    }

    // Update is called once per frame
    void Update()
    {

        float angle = Vector3.Angle(Vector3.up, transform.up);

        //Debug.Log("Angle " + angle);

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");


        if (angle > maxInclination && rb.velocity.magnitude > safeVelocity)
        {
            rocket_state = ROCKET_STATES.TILTED | ROCKET_STATES.FAST;
        }
        if (rb.velocity.magnitude > safeVelocity)
        {
            rocket_state =  ROCKET_STATES.FAST;
        }
        else if (angle > maxInclination)
        {
            rocket_state = ROCKET_STATES.TILTED;
        }
        else
        {
            rocket_state = ROCKET_STATES.STEADY;
        }        


        velocityText.text = "Velocity: " + System.Math.Round(rb.velocity.magnitude* 3.6f, 2) + " Km / h";

        Debug.Log(Mathf.Abs(x));

        if (x != 0)
        {
            if (x > 0)
            {
                //rb.AddForce(new Vector3(10, 0, 0)); // Versão inicial para testes
                //rb.AddForceAtPosition(new Vector3(auxiliarTrustForce, 0, 0), leftThrust.transform.localPosition); //Não está correto, a força está sendo aplicada em uma posição estranha e faz a nave girar sem controle
                rb.AddForceAtPosition(transform.up* Mathf.Abs(x) * auxiliarTrustForce, leftThrust.transform.position, ForceMode.Force); // Já é possível controlar o foguete
                leftThrust.SetActive(true);
            }
            else
            {
                //rb.AddForce(new Vector3(-10, 0, 0));
                //rb.AddForceAtPosition(new Vector3(-auxiliarTrustForce, 0, 0), rightThrust.transform.localPosition);
                rb.AddForceAtPosition(transform.up* Mathf.Abs(x) * auxiliarTrustForce, rightThrust.transform.position, ForceMode.Force);
                rightThrust.SetActive(true);
            }
        }
        else
        {
            leftThrust.SetActive(false);
            rightThrust.SetActive(false);
        }

        if (y != 0)
        {
            if (y > 0)
            {
                //rb.AddForce(new Vector3(0, 10, 0));
                rb.AddForceAtPosition(transform.up* centerThrustForce, centerThrust.transform.position, ForceMode.Force);
                centerThrust.SetActive(true);
            }
            else
            {
                //rb.AddForce(new Vector3(0, -10, 0));
            }
        }
        else
        {
            centerThrust.SetActive(false);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (rocket_state != ROCKET_STATES.STEADY)
        {
            rocket_state = ROCKET_STATES.CRASHED;
            gameObject.SetActive(false);

            Debug.Log("rocket_state " + rocket_state + "object " + other.gameObject.name);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (rocket_state != ROCKET_STATES.STEADY)
        {
            rocket_state = ROCKET_STATES.CRASHED;
            gameObject.SetActive(false);

            Debug.Log("rocket_state " + rocket_state + "object " + other.gameObject.name);
        }
    }
}


