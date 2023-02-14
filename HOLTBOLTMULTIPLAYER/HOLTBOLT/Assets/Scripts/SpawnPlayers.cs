using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
    public static SpawnPlayers instance;
    public Transform[] spawnPoints;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform spawn in spawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0,spawnPoints.Length)];
    }
}
