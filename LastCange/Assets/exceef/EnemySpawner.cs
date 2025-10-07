using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] enemyPrefabs; // ← sekarang array prefab
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
                // Pilih prefab musuh secara acak dari daftar
                GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

                // Pilih titik spawn secara acak
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

                // Spawn musuh di posisi spawn point
                GameObject newEnemy = Instantiate(randomEnemyPrefab, spawnPoint.position, Quaternion.identity);

                // Set referensi player kalau skrip musuh butuh
                EnemyIMO enemyScript = newEnemy.GetComponent<EnemyIMO>();
                if (enemyScript != null)
                {
                    // optional: kalau script Monster butuh referensi player
                    // enemyScript.SetPlayer(player);
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
