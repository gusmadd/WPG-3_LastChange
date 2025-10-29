using System.Collections;
using UnityEngine;

public class EnemySpawnerIMO : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab; // prefab EnemyIMO
    public Transform player;       // referensi player
    public float spawnInterval = 3f; // waktu spawn antar enemy
    public float spawnRadius = 8f;   // radius spawn di sekitar player
    public int maxEnemies = 10;      // batas musuh

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
                Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle * spawnRadius;
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

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
