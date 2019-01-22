using UnityEngine;
using MLAgents;
using System;

public class LanderAgent : Agent
{
    public LanderEnviroment landerEnviroment;

    public float auxiliarTrustForce = 5.0f, centerThrustForce = 10.0f;
    public float safeVelocity = 5.0f; // Meters per second
    public float maxInclination = 15.0f;

    //float angle = 0.0f;

    public GameObject target;

    [Space]
    public GameObject leftThrust, centerThrust, rightThrust;
    bool isCrashed = false;
    bool isLanded = false;

    [SerializeField]
    private GameObject enviroment; //This way we can have multiple enviroments at the same time to improve learning time

    [SerializeField]
    private GameObject island; //To be able to track the islando position we need to store a reference

    Rigidbody agentRB; // 
    Vector3 startPosition; // 
    
    Quaternion startRotation;       
    
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
    Vector3 direction; // state 9 (x) | state 10 (y)
    float distance; // state 11

    public override void InitializeAgent()
    {
        base.InitializeAgent();

        agentRB = GetComponent<Rigidbody>();
        startPosition = agentRB.position;
        startRotation = agentRB.rotation;

        landerEnviroment.SetIslandOnARandomPosition();
        UpdateValues();
    }

    void UpdateValues()
    {
        relativePosX = transform.localPosition.x - enviroment.transform.position.x; // state 1

        relativePosY = transform.localPosition.y - enviroment.transform.position.y; // state 2

        islandRelativePosX = island.transform.position.x - enviroment.transform.position.x; // state 3

        islandRelativePosY = island.transform.position.y - enviroment.transform.position.y; // state 4

        rocketVelocityX = agentRB.velocity.x; // state 5

        rocketVelocityY = agentRB.velocity.y; // state 6

        rocketAlignment = Vector3.Dot(Vector3.right, -transform.up); // state 7

        rocketAngularVelocityZ = agentRB.angularVelocity.z; // state 8

        direction = target.transform.position - transform.position; // state 9 (x) | state 10 (y)

        distance = Vector2.Distance(new Vector2(islandRelativePosX, islandRelativePosY), new Vector2(relativePosX, relativePosY)); // state 11

        //Debug.DrawRay(transform.position, direction.normalized * 3, Color.black);
        //Debug.Log("direction " + direction.normalized.ToString());   
    }

    public override void CollectObservations()
    {
        AddVectorObs(relativePosX); // state 1
        
        AddVectorObs(relativePosY); // state 2
               
        AddVectorObs(islandRelativePosX); // state 3
        
        AddVectorObs(islandRelativePosY); // state 4
        
        AddVectorObs(rocketVelocityX); // state 5
        
        AddVectorObs(rocketVelocityY); // state 6
        
        AddVectorObs(rocketAlignment); // state 7
      
        AddVectorObs(rocketAngularVelocityZ); // state 8
       
        AddVectorObs(direction.normalized.x); // state 9

        AddVectorObs(direction.normalized.y); // state 10

        AddVectorObs(distance); // state 11

        Monitor.Log("Rocket relative position X ", relativePosX.ToString());
        Monitor.Log("Rocket relative position Y ", relativePosY.ToString());
        Monitor.Log("Island relative position X ", islandRelativePosX.ToString());
        Monitor.Log("Island relative position Y ", islandRelativePosY.ToString());
        Monitor.Log("Rocket velocity X ", rocketVelocityX.ToString());
        Monitor.Log("Rocket velocity Y ", rocketVelocityY.ToString());
        Monitor.Log("Rocket alignment ", rocketAlignment.ToString());
        Monitor.Log("Rocket angular velocity Z ", rocketAngularVelocityZ.ToString());
        Monitor.Log("Cumulative reward ", GetCumulativeReward().ToString());

        string debugString = string.Empty;

        debugString += "relativePosX " + relativePosX.ToString() + "\n";
        debugString += "relativePosY " + relativePosY.ToString() + "\n";
        debugString += "islandRelativePosX " + islandRelativePosX.ToString() + "\n";
        debugString += "islandRelativePosY " + islandRelativePosY.ToString() + "\n";
        debugString += "rocketVelocityX " + rocketVelocityX.ToString() + "\n";
        debugString += "rocketVelocityY " + rocketVelocityY.ToString() + "\n";
        debugString += "rocketAlignment " + rocketAlignment.ToString() + "\n";
        debugString += "rocketAngularVelocityZ " + rocketAngularVelocityZ.ToString() + "\n";
        debugString += "direction x " + direction.normalized.x.ToString() + "\n";
        debugString += "direction y " + direction.normalized.y.ToString() + "\n";
        debugString += "distance " + distance.ToString() + "\n\n";

        Debug.Log(debugString);
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
        UpdateValues();
        //Unity bug
        if (agentRB.constraints == RigidbodyConstraints.FreezeAll)
        {
            agentRB.constraints = RigidbodyConstraints.None;
            agentRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;

        }

    }

    void CheckAgentState()
    {

        AddReward(-0.005f); // Finish as fast as possible

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
            Done();
            AddReward(-1f);
        }
        else
        {
            AddReward(0.005f);
        }      
        
        if (distance < lastDistance)
        {           
            if (distance > 0)
            {
                //Debug.Log("Moving Closer " + (0.1f / distance));
                AddReward(0.1f / distance);
            }            
        }
        else
        {
            //Debug.Log("Moving Away " + (-0.001f * distance));
            AddReward(-0.01f);
            
        }

        lastDistance = distance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsSpeedOk() || other.gameObject.tag != "goal")
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