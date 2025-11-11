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
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (currentEnemies < maxEnemies && !isSpawning)
            {
                yield return StartCoroutine(SpawnOneEnemy());
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator SpawnOneEnemy()
    {
        isSpawning = true;

        // hitung posisi spawn di sekitar player
        Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle * spawnRadius;

        // Buat efek spawn
        GameObject spawnFx = new GameObject("SpawnEffect");
        var sr = spawnFx.AddComponent<SpriteRenderer>();
        var fx = spawnFx.AddComponent<SpawnEffect>();
        fx.frames = spawnEffectFrames;
        fx.frameRate = frameRate;
        fx.destroyOnEnd = true;
        spawnFx.transform.position = spawnPos;

        // Tunggu durasi efek spawn (1 detik)
        float totalDuration = spawnEffectFrames.Length * frameRate;
        yield return new WaitForSeconds(totalDuration);

        // Spawn musuh setelah efek selesai
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        EnemyIMO enemyScript = newEnemy.GetComponent<EnemyIMO>();
        if (enemyScript != null)
            enemyScript.player = player;

        currentEnemies++;
        isSpawning = false;
    }

    public void EnemyDied()
    {
        currentEnemies--;
        if (currentEnemies < 0) currentEnemies = 0;
    }

    // 🩸 Fungsi baru: Spawn langsung saat player kebakar
    public void SpawnImmediateOnPlayerBurn()
    {
        if (enemyPrefab == null || player == null) return;

        // Boleh spawn walaupun sudah menyentuh maxEnemies (biar gak ganggu spawner utama)
        Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle * spawnRadius;

        // Efek spawn langsung (tanpa delay)
        GameObject spawnFx = new GameObject("SpawnEffect_Instant");
        var sr = spawnFx.AddComponent<SpriteRenderer>();
        var fx = spawnFx.AddComponent<SpawnEffect>();
        fx.frames = spawnEffectFrames;
        fx.frameRate = frameRate;
        fx.destroyOnEnd = true;
        spawnFx.transform.position = spawnPos;

        // Spawn musuh langsung setelah efek muncul
        StartCoroutine(SpawnAfterDelay(spawnPos));
    }

    IEnumerator SpawnAfterDelay(Vector2 pos)
    {
        float totalDuration = spawnEffectFrames.Length * frameRate;
        yield return new WaitForSeconds(totalDuration);

        GameObject newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        EnemyIMO enemyScript = newEnemy.GetComponent<EnemyIMO>();
        if (enemyScript != null)
            enemyScript.player = player;

        currentEnemies++;
    }
}
