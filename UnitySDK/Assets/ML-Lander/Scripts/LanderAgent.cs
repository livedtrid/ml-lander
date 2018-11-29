using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;

public class LanderAgent : Agent
{

    public float auxiliarTrustForce = 5.0f, centerThrustForce = 10.0f;
    public float safeVelocity = 5.0f; // Meters per second
    public float maxInclination = 15.0f;

    float angle = 0.0f;

    public GameObject target;

    [Space]
    public GameObject leftThrust, centerThrust, rightThrust;
    bool isCrashed = false;
    bool isLanded = false;

    LanderAcademy academy;

    Rigidbody agentRB;
    Vector3 startPosition;
    Quaternion startRotation;

    //Vector3 distance = default(Vector3);
    float distance = 0;
    float lastDistance = 1000f;

    private const int NoAction = 0;  // do nothing!
    private const int Up = 1;
    private const int Left = 2;
    private const int Right = 3;

    [System.Flags]
    public enum AGENT_STATES : int { STEADY = 1 << 0, TILTED = 1 << 1, FAST = 1 << 2, CRASHED = 1 << 3, LANDED = 1 << 4 }
    public AGENT_STATES agent_state = AGENT_STATES.STEADY;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        academy = FindObjectOfType(typeof(LanderAcademy)) as LanderAcademy;

        agentRB = GetComponent<Rigidbody>();
        startPosition = agentRB.position;
        startRotation = agentRB.rotation;

    }

    public override void CollectObservations()
    {

        AddVectorObs(agentRB.velocity); //Add the rocket velocity to the observation vector
        AddVectorObs(distance); //Distance to the landing zone
        AddVectorObs(angle);

        Monitor.Log("Rocket velocity ", agentRB.velocity.ToString());
        Monitor.Log("Rocket distance to the target ", distance.ToString());
        Monitor.Log("Rocket inclination ", angle.ToString() + "º");
        Monitor.Log("Total Reward ", GetCumulativeReward().ToString());
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Action vector should be set on the brain

        MoveAgent(vectorAction);
        CheckAgentState();

        //Debug.Log("vectorAction[0] " + vectorAction[0].ToString());
        //Debug.Log("agent_state " + agent_state.ToString());

        switch (agent_state)
        {
            case AGENT_STATES.STEADY:
                AddReward(0.1f);
                break;
            case AGENT_STATES.TILTED:
                AddReward(-0.001f);
                break;
            case AGENT_STATES.FAST:
                AddReward(-0.001f);
                break;
            case AGENT_STATES.CRASHED:
                AddReward(-1f);
                Done();
                break;
            case AGENT_STATES.LANDED:
                AddReward(1f);
                Done();
                break;
            default:
                break;
        }
    }



    public override void AgentReset()
    {
        isCrashed = false;
        isLanded = false;
        agentRB.velocity = default(Vector3);
        agentRB.angularVelocity = default(Vector3);
        agentRB.position = startPosition;
        agentRB.rotation = startRotation;


    }

    public override void AgentOnDone()
    {

    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            //HOW??? vec[0] = force | vec[1] = direction?
        }
        else
        {
            int action = Mathf.FloorToInt(act[0]);

            switch (action)
            {
                case NoAction:
                    // do nothing
                    centerThrust.SetActive(false);
                    leftThrust.SetActive(false);
                    rightThrust.SetActive(false);
                    break;
                case Up: //Thrust UP
                    agentRB.AddForceAtPosition(transform.up * centerThrustForce, centerThrust.transform.position, ForceMode.Force);
                    centerThrust.SetActive(true);
                    break;
                case Left: //Thrust LEFT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, leftThrust.transform.position, ForceMode.Force);
                    leftThrust.SetActive(true);
                    break;
                case Right: //Thrust RIGHT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, rightThrust.transform.position, ForceMode.Force);
                    rightThrust.SetActive(true);
                    break;
                default:
                    throw new ArgumentException("Invalid action value");
            }
        }
    }

    private void FixedUpdate()
    {

    }

    void CheckAgentState()
    {
        angle = Vector3.Angle(Vector3.up, transform.up);
        //distance = target.transform.position - transform.position;
        distance = Vector3.Distance(target.transform.position, transform.position);

        if (distance < lastDistance)
        {
            AddReward(0.01f);
        }
        else
        {
            AddReward(-0.01f);
        }

        lastDistance = distance;

        if (!isCrashed && !isLanded)
        {
            if (agentRB.velocity.magnitude > safeVelocity)
            {
                agent_state = AGENT_STATES.FAST;
            }
            else if (angle > maxInclination)
            {
                agent_state = AGENT_STATES.TILTED;
            }
            else
            {
                agent_state = AGENT_STATES.STEADY;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (agent_state != AGENT_STATES.STEADY || other.gameObject.tag != "goal")
        {
            isCrashed = true;
            agent_state = AGENT_STATES.CRASHED;
            Debug.Log("rocket_state " + agent_state + " object " + other.gameObject.name);
        }
        else
        {
            agent_state = AGENT_STATES.LANDED;
            isLanded = true;
            Debug.Log("rocket_state " + agent_state + " object " + other.gameObject.name);
        }
    }
}
