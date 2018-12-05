using UnityEngine;
using MLAgents;
using System;

public class LanderAgent : Agent
{
    public LanderEnviroment landerEnviroment;

    public float auxiliarTrustForce = 5.0f, centerThrustForce = 10.0f;
    public float safeVelocity = 5.0f; // Meters per second
    public float maxInclination = 15.0f;

    float angle = 0.0f;

    public GameObject target;

    [Space]
    public GameObject leftThrust, centerThrust, rightThrust;
    bool isCrashed = false;
    bool isLanded = false;

    [SerializeField]
    private GameObject enviroment; //This way we can have multiple enviroments at the same time to improve learning time

    [SerializeField]
    private GameObject island;
    //LanderAcademy academy;

    Rigidbody agentRB;
    Vector3 startPosition;
    Vector3 direction;
    Quaternion startRotation;

    //Vector3 distance = default(Vector3);
    float distance = 0;
    float lastDistance = float.PositiveInfinity;

    private const int NoAction = 0;  // do nothing!
    private const int Up = 1;
    private const int Left = 2;
    private const int Right = 3;


    //Agent states
    float relativePosX; // state 1
    float relativePosY; // state 2   
    float islandRelativePosX; // state 3
    float islandRelativePosY; // state 4
    float rocketVelocityX; // state 5
    float rocketVelocityY; // state 6
    float rocketAlignment; // state 7
    float rocketAngularVelocityZ; // state 8

    [System.Flags]
    public enum AGENT_STATES : int { STEADY = 1 << 0, TILTED = 1 << 1, FAST = 1 << 2, CRASHED = 1 << 3, LANDED = 1 << 4 }
    public AGENT_STATES agent_state = AGENT_STATES.STEADY;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        //academy = FindObjectOfType(typeof(LanderAcademy)) as LanderAcademy;

        agentRB = GetComponent<Rigidbody>();
        startPosition = agentRB.position;
        startRotation = agentRB.rotation;

        landerEnviroment.SetIslandOnARandomPosition();
    }

    public override void CollectObservations()
    {
        //The agent relative position
        relativePosX = transform.localPosition.x - enviroment.transform.position.x;
        AddVectorObs(relativePosX); // state 1

        relativePosY = transform.localPosition.y - enviroment.transform.position.y;
        AddVectorObs(relativePosY); // state 2

        // The landing zone relative position
        islandRelativePosX = island.transform.position.x - enviroment.transform.position.x;
        AddVectorObs(islandRelativePosX); // state 3

        islandRelativePosY = island.transform.position.y - enviroment.transform.position.y;
        AddVectorObs(islandRelativePosY); // state 4

        rocketVelocityX = agentRB.velocity.x;
        AddVectorObs(rocketVelocityX); // state 5

        rocketVelocityY = agentRB.velocity.y;
        AddVectorObs(rocketVelocityY); // state 6

        rocketAlignment = Vector3.Dot(Vector3.right, -transform.up);
        AddVectorObs(rocketAlignment); // state 7

        rocketAngularVelocityZ = agentRB.angularVelocity.z;
        AddVectorObs(rocketAngularVelocityZ); // state 8


        Monitor.Log("Rocket relative position X ", relativePosX.ToString());
        Monitor.Log("Rocket relative position Y ", relativePosY.ToString());
        Monitor.Log("Island relative position X ", islandRelativePosX.ToString());
        Monitor.Log("Island relative position Y ", islandRelativePosY.ToString());
        Monitor.Log("Rocket velocity X ", rocketVelocityX.ToString());
        Monitor.Log("Rocket velocity Y ", rocketVelocityY.ToString());
        Monitor.Log("Rocket alignment ", rocketAlignment.ToString());
        Monitor.Log("Rocket angular velocity Z ", rocketAngularVelocityZ.ToString());
        Monitor.Log("Cumulative reward ", GetCumulativeReward().ToString());
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        CheckAgentState();
        MoveAgent(vectorAction);
    }

    public override void AgentReset()
    {
        landerEnviroment.SetIslandOnARandomPosition();

        agentRB.constraints = RigidbodyConstraints.FreezeAll;

        isCrashed = false;
        isLanded = false;
        agentRB.velocity = default(Vector3);
        agentRB.angularVelocity = default(Vector3);
        agentRB.position = startPosition;
        agentRB.rotation = startRotation;
        lastDistance = float.PositiveInfinity;
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
                    agentRB.AddRelativeForce(Vector3.up * centerThrustForce, ForceMode.Acceleration);
                    centerThrust.SetActive(true);
                    break;
                case Left: //Thrust LEFT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, leftThrust.transform.position, ForceMode.Acceleration);
                    leftThrust.SetActive(true);
                    break;
                case Right: //Thrust RIGHT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, rightThrust.transform.position, ForceMode.Acceleration);
                    rightThrust.SetActive(true);
                    break;
                default:
                    throw new ArgumentException("Invalid action value");
            }
        }
    }

    private void FixedUpdate()
    {

        //Debug.Log("Alignment Vector: " + Vector3.Dot(Vector3.right, - transform.up)); //Negative = left Positive = right

        //Unity bug
        if (agentRB.constraints == RigidbodyConstraints.FreezeAll)
        {
            agentRB.constraints = RigidbodyConstraints.None;
            agentRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;

        }

    }

    void CheckAgentState()
    {
        AddReward(-0.005f);

        if (isCrashed)
        {
            Debug.Log("CRASHED");
            Done();
            AddReward(-1f);
        }

        if (isLanded)
        {
            Debug.Log("LANDED");
            Done();
            AddReward(1f);
        }

        if (!IsSpeedOk())
        {
            Debug.Log("Too Fast");
            Done();
            AddReward(-1f);
        }
        else
        {
            AddReward(0.005f);
        }

        if (Math.Abs(rocketAngularVelocityZ) > 0.8)
        {
            Debug.Log("Too much angular velocity");
            Done();
            AddReward(-1f);
        }
        else
        {
            AddReward(0.005f);
        }

        angle = Vector3.Angle(Vector3.up, transform.up);
        distance = Vector3.Distance(target.transform.position, transform.position);
        direction = target.transform.position - transform.position;

        if (distance < lastDistance)
        {
            //Debug.Log("Moving Closer");
        }
        else
        {
            //Debug.Log("Moving Away");
        }

        lastDistance = distance;


        if (!isCrashed && !isLanded)
        {
            if (Math.Abs(agentRB.velocity.y) > safeVelocity)
            {
                agent_state = AGENT_STATES.FAST;
            }
            //else if (angle > maxInclination)
            //{
            //    agent_state = AGENT_STATES.TILTED;
            //}
            else
            {
                agent_state = AGENT_STATES.STEADY;
            }
        }
        else
        {

        }

        float dx = relativePosX - islandRelativePosX;
        float dy = relativePosY - islandRelativePosY;

        Debug.Log("Distance X " + 1f/dx);
        Debug.Log("Distance Y " + 1f/dy);

        AddReward(1f / dx);
        AddReward(1f / dy);

        //Debug.Log("agent_state " + agent_state.ToString());

        //Debug.Log("agent_state " + agent_state.ToString());
        //Debug.Log("agentRB.velocity.magnitude  " + agentRB.velocity.y);
        //Debug.Log("safeVelocity  " + safeVelocity);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (agent_state != AGENT_STATES.STEADY || other.gameObject.tag != "goal")
        {
            isCrashed = true;
        }
        else
        {
            isLanded = true;
        }
    }

    private bool IsSpeedOk()
    {
        return Math.Abs(rocketVelocityX) < 6.5 && Math.Abs(rocketVelocityY) < 6.5;
    }
}





/*
 
     
     
     
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

    [SerializeField]
    private GameObject enviroment; //This way we can have multiple enviroments at the same time to improve learning time

    [SerializeField]
    private GameObject island;
    //LanderAcademy academy;

    Rigidbody agentRB;
    Vector3 startPosition;
    Vector3 direction;
    Quaternion startRotation;

    //Vector3 distance = default(Vector3);
    float distance = 0;
    float lastDistance = float.PositiveInfinity;

    private const int NoAction = 0;  // do nothing!
    private const int Up = 1;
    private const int Left = 2;
    private const int Right = 3;


    //Agent states
    float relativePosX; // state 1
    float relativePosY; // state 2   
    float islandRelativePosX; // state 3
    float islandRelativePosY; // state 4
    float rocketVelocityX; // state 5
    float rocketVelocityY; // state 6
    float rocketAlignment; // state 7
    float rocketAngularVelocityZ; // state 8

    [System.Flags]
    public enum AGENT_STATES : int { STEADY = 1 << 0, TILTED = 1 << 1, FAST = 1 << 2, CRASHED = 1 << 3, LANDED = 1 << 4 }
    public AGENT_STATES agent_state = AGENT_STATES.STEADY;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        //academy = FindObjectOfType(typeof(LanderAcademy)) as LanderAcademy;

        agentRB = GetComponent<Rigidbody>();
        startPosition = agentRB.position;
        startRotation = agentRB.rotation;
    }

    public override void CollectObservations()
    {
        //The agent relative position
        relativePosX = transform.localPosition.x - enviroment.transform.position.x;
        AddVectorObs(relativePosX); // state 1

        relativePosY = transform.localPosition.y - enviroment.transform.position.y;
        AddVectorObs(relativePosY); // state 2

        // The landing zone relative position
        islandRelativePosX = island.transform.position.x - enviroment.transform.position.x;
        AddVectorObs(islandRelativePosX); // state 3

        islandRelativePosY = island.transform.position.y - enviroment.transform.position.y;
        AddVectorObs(islandRelativePosY); // state 4

        rocketVelocityX = agentRB.velocity.x;
        AddVectorObs(rocketVelocityX); // state 5

        rocketVelocityY = agentRB.velocity.y;
        AddVectorObs(rocketVelocityY); // state 6

        rocketAlignment = Vector3.Dot(Vector3.right, -transform.up);
        AddVectorObs(rocketAlignment); // state 7

        rocketAngularVelocityZ = agentRB.angularVelocity.z;
        AddVectorObs(rocketAngularVelocityZ); // state 8


        Monitor.Log("Rocket relative position X ", relativePosX.ToString());
        Monitor.Log("Rocket relative position Y ", relativePosY.ToString());
        Monitor.Log("Island relative position X ", islandRelativePosX.ToString());
        Monitor.Log("Island relative position Y ", islandRelativePosY.ToString());
        Monitor.Log("Rocket velocity X ", rocketVelocityX.ToString());
        Monitor.Log("Rocket velocity Y ", rocketVelocityY.ToString());
        Monitor.Log("Rocket alignment ", rocketAlignment.ToString());
        Monitor.Log("Rocket angular velocity Z ", rocketAngularVelocityZ.ToString());
        Monitor.Log("Cumulative reward ", GetCumulativeReward().ToString());
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        CheckAgentState();
        MoveAgent(vectorAction);
    }

    public override void AgentReset()
    {
        agentRB.constraints = RigidbodyConstraints.FreezeAll;

        isCrashed = false;
        isLanded = false;
        agentRB.velocity = default(Vector3);
        agentRB.angularVelocity = default(Vector3);
        agentRB.position = startPosition;
        agentRB.rotation = startRotation;
        lastDistance = float.PositiveInfinity;
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
                    agentRB.AddRelativeForce(Vector3.up * centerThrustForce, ForceMode.Acceleration);
                    centerThrust.SetActive(true);
                    break;
                case Left: //Thrust LEFT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, leftThrust.transform.position, ForceMode.Acceleration);
                    leftThrust.SetActive(true);
                    break;
                case Right: //Thrust RIGHT
                    agentRB.AddForceAtPosition(transform.up * auxiliarTrustForce, rightThrust.transform.position, ForceMode.Acceleration);
                    rightThrust.SetActive(true);
                    break;
                default:
                    throw new ArgumentException("Invalid action value");
            }
        }
    }

    private void FixedUpdate()
    {

        //Debug.Log("Alignment Vector: " + Vector3.Dot(Vector3.right, - transform.up)); //Negative = left Positive = right

        //Unity bug
        if (agentRB.constraints == RigidbodyConstraints.FreezeAll)
        {
            agentRB.constraints = RigidbodyConstraints.None;
            agentRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;

        }

    }

    void CheckAgentState()
    {
        AddReward(-0.005f);

        //AddReward(-1f / agentParameters.maxStep);

        angle = Vector3.Angle(Vector3.up, transform.up);
        distance = Vector3.Distance(target.transform.position, transform.position);
        direction = target.transform.position - transform.position;

        //AddReward(1f / distance);
        //AddReward(-0.01f * Vector3.Dot(Vector3.up, -transform.up)); //Check the inclination
        //AddReward(-0.0001f * Math.Abs(agentRB.velocity.y));

        //Debug.Log("AddReward distance; " + -0.001f * distance + " AddReward direction; " + 0.01f * Vector3.Dot(direction.normalized, -transform.up));
        //Debug.Log("distance; " + distance + " lastDistance: " + lastDistance);
        //Debug.Log("direction.magnitude; " + direction.magnitude);
        //Debug.Log("velocity; " + agentRB.velocity.sqrMagnitude);

        if (distance < lastDistance)
        {
            //Debug.Log("Moving Closer");
        }
        else
        {
            //Debug.Log("Moving Away");
        }

        lastDistance = distance;


        if (!isCrashed && !isLanded)
        {
            if (Math.Abs(agentRB.velocity.y) > safeVelocity)
            {
                agent_state = AGENT_STATES.FAST;
            }
            //else if (angle > maxInclination)
            //{
            //    agent_state = AGENT_STATES.TILTED;
            //}
            else
            {
                agent_state = AGENT_STATES.STEADY;
            }
        }
        else
        {
            if (isCrashed)
            {
                agent_state = AGENT_STATES.CRASHED;

                Done();
                AddReward(-1f);

            }

            if (isLanded)
            {
                agent_state = AGENT_STATES.LANDED;

                Done();
                AddReward(1f);
            }
        }

        //Debug.Log("agent_state " + agent_state.ToString());

        //Debug.Log("agent_state " + agent_state.ToString());
        //Debug.Log("agentRB.velocity.magnitude  " + agentRB.velocity.y);
        //Debug.Log("safeVelocity  " + safeVelocity);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (agent_state != AGENT_STATES.STEADY || other.gameObject.tag != "goal")
        {
            isCrashed = true;
        }
        else
        {
            isLanded = true;
        }
    }
}

     
     
     
     */













