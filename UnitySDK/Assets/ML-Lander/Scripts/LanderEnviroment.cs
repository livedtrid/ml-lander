using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanderEnviroment : MonoBehaviour {

    public Transform islandTransform;
    public Transform rocketTransform;

    public void SetIslandOnARandomPosition()
    {
        float randomX = Random.Range(-45.0f, 45.0f);

        islandTransform.position = new Vector3((int)randomX, islandTransform.position.y, islandTransform.position.z);
    }

    public void SetRocketRandomPosition()
    {
        float randomX = Random.Range(-40.0f, 40.0f);
        float randomY = Random.Range(25.0f, 35.0f);

        rocketTransform.position = new Vector3((int)randomX, (int)randomY, rocketTransform.position.z);
    }
}
