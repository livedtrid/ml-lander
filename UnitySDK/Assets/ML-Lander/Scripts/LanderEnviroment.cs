using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanderEnviroment : MonoBehaviour {

    public Transform islandTransform;

    public void SetIslandOnARandomPosition()
    {
        float randomX = Random.Range(-45.0f, 45.0f);

        islandTransform.position = new Vector3((int)randomX, islandTransform.position.y, islandTransform.position.z);
    }
}
