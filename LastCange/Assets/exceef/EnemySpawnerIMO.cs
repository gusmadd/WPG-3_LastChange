using System.Collections;
using UnityEngine;

public class EnemySpawnerIMO : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;     // prefab EnemyIMO
    public Transform player;           // referensi player
    public float spawnInterval = 3f;   // waktu spawn antar enemy
    public float spawnRadius = 8f;     // radius spawn di sekitar player
    public int maxEnemies = 10;        // batas musuh

    [Header("Spawn Effect")]
    public Sprite[] spawnEffectFrames;  // 19 sprite dari SpawnEffect
    public float frameRate = 0.05f;     // durasi tiap frame (sama kayak di SpawnEffect)
    private int currentEnemies = 0;

    void Start()
    {
        // Jalankan satu coroutine global
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // hanya spawn kalau belum penuh
            if (currentEnemies < maxEnemies)
            {
                yield return StartCoroutine(SpawnOneEnemy());
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator SpawnOneEnemy()
    {
        // hitung posisi spawn
        Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle * spawnRadius;

        // Buat efek spawn
        GameObject spawnFx = new GameObject("SpawnEffect");
        var sr = spawnFx.AddComponent<SpriteRenderer>();
        var fx = spawnFx.AddComponent<SpawnEffect>();
        fx.frames = spawnEffectFrames;
        fx.frameRate = frameRate;
        fx.destroyOnEnd = true;
        spawnFx.transform.position = spawnPos;

        // Tunggu durasi efek (1 detik)
        float totalDuration = spawnEffectFrames.Length * frameRate;
        yield return new WaitForSeconds(totalDuration);

        // Spawn musuh setelah efek selesai
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        EnemyIMO enemyScript = newEnemy.GetComponent<EnemyIMO>();
        if (enemyScript != null)
        {
            enemyScript.player = player;
        }

        currentEnemies++;
    }

    public void EnemyDied()
    {
        currentEnemies--;
        if (currentEnemies < 0) currentEnemies = 0;
    }
}
