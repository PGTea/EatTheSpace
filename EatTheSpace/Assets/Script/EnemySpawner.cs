using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] insideCameraPrefabs;
    [SerializeField] private GameObject[] outsideCameraPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float insideSpawnInterval = 3f;
    [SerializeField] private float outsideSpawnInterval = 2f;

    [Header("Pool Settings")]
    [SerializeField] private int maxInsideEnemies = 10;
    [SerializeField] private int maxOutsideEnemies = 20;

    private float insideSpawnTimer;
    private float outsideSpawnTimer;
    private Camera mainCam;
    private List<GameObject> insideEnemyPool;
    private List<GameObject> outsideEnemyPool;

    /// <summary>
    /// Initializes the camera and object pools.
    /// </summary>
    void Start()
    {
        mainCam = Camera.main;
        InitializePools();
    }

    /// <summary>
    /// Initializes the object pools for inside and outside enemies.
    /// </summary>
    private void InitializePools()
    {
        insideEnemyPool = new List<GameObject>();
        for (int i = 0; i < maxInsideEnemies; i++)
        {
            GameObject prefabToSpawn = insideCameraPrefabs[Random.Range(0, insideCameraPrefabs.Length)];
            GameObject obj = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity, transform);
            obj.SetActive(false);
            insideEnemyPool.Add(obj);
        }

        outsideEnemyPool = new List<GameObject>();
        for (int i = 0; i < maxOutsideEnemies; i++)
        {
            GameObject prefabToSpawn = outsideCameraPrefabs[Random.Range(0, outsideCameraPrefabs.Length)];
            GameObject obj = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity, transform);
            obj.SetActive(false);
            outsideEnemyPool.Add(obj);
        }
    }

    /// <summary>
    /// Handles the spawn timers and triggers spawning.
    /// </summary>
    void Update()
    {
        insideSpawnTimer += Time.deltaTime;
        if (insideSpawnTimer >= insideSpawnInterval)
        {
            insideSpawnTimer = 0f;
            SpawnInsideCamera();
        }

        outsideSpawnTimer += Time.deltaTime;
        if (outsideSpawnTimer >= outsideSpawnInterval)
        {
            outsideSpawnTimer = 0f;
            SpawnOutsideCamera();
        }
    }

    /// <summary>
    /// Spawns an enemy inside the camera view.
    /// </summary>
    private void SpawnInsideCamera()
    {
        GameObject enemyToSpawn = GetPooledObject(insideEnemyPool);
        if (enemyToSpawn == null) return;

        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;
        Vector2 camPos = mainCam.transform.position;

        float randomX = Random.Range(camPos.x - camWidth, camPos.x + camWidth);
        float randomY = Random.Range(camPos.y - camHeight, camPos.y + camHeight);
        Vector2 spawnPos = new Vector2(randomX, randomY);

        enemyToSpawn.transform.position = spawnPos;
        enemyToSpawn.transform.rotation = Quaternion.identity;
        enemyToSpawn.SetActive(true);
    }

    /// <summary>
    /// Spawns an enemy just outside the camera view.
    /// </summary>
    private void SpawnOutsideCamera()
    {
        GameObject enemyToSpawn = GetPooledObject(outsideEnemyPool);
        if (enemyToSpawn == null) return;

        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;
        Vector2 camPos = mainCam.transform.position;

        float margin = 2f;
        Enemy enemyScript = enemyToSpawn.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            margin = enemyScript.OutsideSpawnMargin;
        }

        int edge = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        switch (edge)
        {
            case 0: spawnPos = new Vector2(Random.Range(camPos.x - camWidth - margin, camPos.x + camWidth + margin), camPos.y + camHeight + margin); break;
            case 1: spawnPos = new Vector2(Random.Range(camPos.x - camWidth - margin, camPos.x + camWidth + margin), camPos.y - camHeight - margin); break;
            case 2: spawnPos = new Vector2(camPos.x - camWidth - margin, Random.Range(camPos.y - camHeight - margin, camPos.y + camHeight + margin)); break;
            case 3: spawnPos = new Vector2(camPos.x + camWidth + margin, Random.Range(camPos.y - camHeight - margin, camPos.y + camHeight + margin)); break;
        }

        enemyToSpawn.transform.position = spawnPos;
        enemyToSpawn.transform.rotation = Quaternion.identity;
        enemyToSpawn.SetActive(true);
    }

    /// <summary>
    /// Retrieves an available object from the provided pool.
    /// </summary>
    private GameObject GetPooledObject(List<GameObject> pool)
    {
        int currentLength = 0;
        if (SnakeController.Instance != null)
        {
            currentLength = SnakeController.Instance.GetLength();
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                Enemy enemy = pool[i].GetComponent<Enemy>();
                if (enemy == null || currentLength >= enemy.LengthToSpawn)
                {
                    return pool[i];
                }
            }
        }
        return null;
    }
}
