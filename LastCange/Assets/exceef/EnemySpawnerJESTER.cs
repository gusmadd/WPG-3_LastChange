using System.Collections;
using UnityEngine;

public class EnemySpawnerJESTER : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab; // prefab EnemyJESTER
    public Transform player;       // referensi player
    public float spawnInterval = 4f; // waktu spawn antar enemy
    public float spawnRadius = 10f;  // radius spawn di sekitar player
    public int maxEnemies = 6;       // batas musuh jester (biasanya lebih sedikit)

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

                EnemyJESTER enemyScript = newEnemy.GetComponent<EnemyJESTER>();
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
