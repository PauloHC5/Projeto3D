using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AcornSpawner : MonoBehaviour
{
    [SerializeField] private GameObject acornPrefab;
    [SerializeField] private float minSpawnDelay = 5f;
    [SerializeField] private float maxSpawnDelay = 15f;
    [SerializeField] private int maxAcornsInScene = 10;
    
    private SphereCollider sphereCollider;
    private int currentAcornCount = 0;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Start()
    {
        InvokeRepeating("SpawnAcorn", maxSpawnDelay, Random.Range(minSpawnDelay, maxSpawnDelay));
    }

    private void LateUpdate()
    {
        currentAcornCount = GameObject.FindGameObjectsWithTag("AcornAmmo").Length;
        if (currentAcornCount >= maxAcornsInScene)
        {
            CancelInvoke("SpawnAcorn");
        }
        else if (!IsInvoking("SpawnAcorn"))
        {
            InvokeRepeating("SpawnAcorn", Random.Range(minSpawnDelay, maxSpawnDelay), Random.Range(minSpawnDelay, maxSpawnDelay));
        }
    }

    private void SpawnAcorn()
    {
        Vector3 randomPoint = Random.insideUnitSphere * sphereCollider.radius;
        randomPoint += transform.position;

        Instantiate(acornPrefab, randomPoint, Quaternion.identity);
    }
    
}
