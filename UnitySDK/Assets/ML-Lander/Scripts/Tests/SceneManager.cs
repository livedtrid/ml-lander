using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{

    public Transform islandTransform, rocketTransform;

    Vector3 rocketStartPosition;
    Quaternion rocketStartRotation;

    

    //-9.3 50

    // Use this for initialization
    void Start()
    {

        rocketStartPosition = rocketTransform.position;
        rocketStartRotation = rocketTransform.rotation;

      

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("r"))
        {
            rocketTransform.position = rocketStartPosition;
            rocketTransform.rotation = rocketStartRotation;
            SetIslandOnARandomPosition();

            rocketTransform.GetComponent<RocketController>().Reset();

        }

    }

    void SetIslandOnARandomPosition()
    {
        float randomX = Random.Range(-45.0f, 45.0f);

        islandTransform.position = new Vector3(randomX, islandTransform.position.y, islandTransform.position.z);
    }
}
