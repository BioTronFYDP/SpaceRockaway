using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnZombie : MonoBehaviour {

    public float spawnDelay;
    public GameObject zombie;
    
    public Transform[] spawnPoints;


    // Use this for initialization
    void Start() {
        
    }


    public void StartGenerateZombies()
    {
        InvokeRepeating("Spawn", spawnDelay, spawnDelay);
    }


    void Spawn()
    {
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        GameObject newZombie = Instantiate(zombie, spawnPoint.position, spawnPoint.rotation);
        Renderer rend = newZombie.transform.GetChild(0).gameObject.GetComponent<Renderer>();
        float r = Random.Range(0.0f, 0.7f);
        float g = Random.Range(0.0f, 0.7f);
        float b = Random.Range(0.0f, 0.7f);
        rend.material.color = new Color(r, g, b, 0.7f);

    }


}
