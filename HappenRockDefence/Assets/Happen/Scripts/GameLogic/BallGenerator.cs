using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGenerator : MonoBehaviour {

    public GameObject[] ballReferences;
    private GameObject ballObject;
    public Transform[] generationPoints;

    public void GenerateNewBall(int altarPosition)
    {
        Transform generationPoint = generationPoints[altarPosition];
        GameObject newBall = Instantiate(ballObject, generationPoint.position, generationPoint.rotation);
    }

    public void GenerateNewBall(Vector3 generationPoint)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        int randInt = Random.Range(0, ballReferences.Length);
        ballObject = ballReferences[randInt];
        GameObject newBall = Instantiate(ballObject, generationPoint, rotation);
    }

}
