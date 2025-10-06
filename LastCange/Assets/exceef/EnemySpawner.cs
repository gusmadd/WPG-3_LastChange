using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnInterval = 3f;
    public int maxEnemies = 10;

    [Header("Spawn Points")]
    public Transform[] spawnPoints; // array titik spawn

    private int currentEnemies = 0;

    void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (currentEnemies < maxEnemies)
            {
                // Pilih titik spawn secara acak dari daftar
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

                // Spawn musuh di posisi spawn point
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

                EnemyIMO enemyScript = newEnemy.GetComponent<EnemyIMO>();
                if (enemyScript != null)
                {
                    enemyScript.player = player;
                }

                currentEnemies++;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void EnemyDied()
    {
        currentEnemies--;
    }
}
