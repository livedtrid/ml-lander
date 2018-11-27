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

    [Space]
    public GameObject leftThrust, centerThrust, rightThrust;

    LanderAcademy academy;

    Rigidbody agentRB;
    Vector3 startPosition;

    private const int NoAction = 0;  // do nothing!
    private const int Up = 1;
    private const int Left = 2;
    private const int Right = 3;

    [System.Flags]
    public enum AGENT_STATES : int { STEADY = 1 << 0, TILTED = 1 << 1, FAST = 1 << 2, CRASHED = 1 << 3 }
    public AGENT_STATES agent_state = AGENT_STATES.STEADY;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        academy = FindObjectOfType(typeof(LanderAcademy)) as LanderAcademy;
        
        agentRB = GetComponent<Rigidbody>();
        startPosition = agentRB.position;

    }

    public override void CollectObservations()
    {

    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Action vector should be set on the brain

        MoveAgent(vectorAction);

    }

    public override void AgentReset()
    {
        agentRB.velocity = default(Vector3);
        agentRB.angularVelocity = default(Vector3);
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
                    break;
                case Up: //Thrust UP
                    agentRB.AddForceAtPosition(transform.up * centerThrustForce, centerThrust.transform.position, ForceMode.Force);
                    break;
                case Left: //Thrust LEFT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, leftThrust.transform.position, ForceMode.Force);
                    break;
                case Right: //Thrust RIGHT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, rightThrust.transform.position, ForceMode.Force);
                    break;
                default:
                    throw new ArgumentException("Invalid action value");
            }
        }

    }

    void CheckAgentState()
    {
        float angle = Vector3.Angle(Vector3.up, transform.up);

        //Debug.Log("Angle " + angle);

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");


        if (angle > maxInclination && agentRB.velocity.magnitude > safeVelocity)
        {
            agent_state = AGENT_STATES.TILTED | AGENT_STATES.FAST;
        }
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
