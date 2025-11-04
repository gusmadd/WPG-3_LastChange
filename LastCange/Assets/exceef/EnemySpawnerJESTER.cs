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

    [Header("Spawn Effect (frame-based)")]
    public Sprite[] spawnEffectFrames; // isi 19 sprite di Inspector
    public float frameRate = 0.05f;    // durasi tiap frame

    private int currentEnemies = 0;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (!isSpawning && currentEnemies < maxEnemies && player != null)
            {
                isSpawning = true;
                Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle * spawnRadius;
                yield return StartCoroutine(SpawnOneEnemyWithEffect(spawnPos));
                isSpawning = false;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator SpawnOneEnemyWithEffect(Vector2 spawnPos)
    {
        // 1) instantiate enemy but deactivate it so it doesn't act/move yet
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.SetActive(false);

        // assign player reference if present
        EnemyJESTER enemyScript = newEnemy.GetComponent<EnemyJESTER>();
        if (enemyScript != null)
            enemyScript.player = player;

        // 2) play spawn effect frame-by-frame at spawnPos
        if (spawnEffectFrames != null && spawnEffectFrames.Length > 0)
        {
            GameObject fx = new GameObject("SpawnEffect_Jester");
            fx.transform.position = spawnPos;
            SpriteRenderer fxSr = fx.AddComponent<SpriteRenderer>();
            fxSr.sortingOrder = 20; // tampil di depan, ubah kalau perlu

            // play frames
            for (int i = 0; i < spawnEffectFrames.Length; i++)
            {
                fxSr.sprite = spawnEffectFrames[i];
                yield return new WaitForSeconds(frameRate);
            }

            Destroy(fx);
        }
        else
        {
            // jika tidak ada frames, tunggu sedikit agar spawn timing konsisten
            yield return null;
        }

        // 3) activate enemy after effect finished
        newEnemy.SetActive(true);
        currentEnemies++;
    }

    public void EnemyDied()
    {
        currentEnemies--;
        if (currentEnemies < 0) currentEnemies = 0;
    }
}
