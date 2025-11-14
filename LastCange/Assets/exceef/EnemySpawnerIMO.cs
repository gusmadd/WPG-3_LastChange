using System.Collections;
using UnityEngine;

public class EnemySpawnerIMO : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnInterval = 3f;
    public float spawnRadius = 8f;
    public int maxEnemies = 10;

    [Header("Special Spawn Points (on burn)")]
    private Transform[] fixedSpawnPoints; // otomatis cari di scene

    [Header("Spawn Effect (frame-based)")]
    public Sprite[] spawnEffectFrames; // isi 19 sprite di Inspector
    public float frameRate = 0.05f;    // durasi tiap frame

    private int currentEnemies = 0;
    private float timer;
private float minX, maxX, minY, maxY;

void Start()
{
    // Ambil ukuran map otomatis dari SpriteRenderer
    SpriteRenderer map = GameObject.Find("Map").GetComponent<SpriteRenderer>();
    if (map != null)
    {
        minX = map.bounds.min.x;
        maxX = map.bounds.max.x;
        minY = map.bounds.min.y;
        maxY = map.bounds.max.y;
    }
    else
    {
        Debug.LogWarning("⚠️ Map tidak ditemukan, spawn mungkin out of bounds!");
    }
}

Vector3 GetValidSpawnPosition()
{
    int tries = 0;
    while (tries < 10)
    {
        // ambil posisi random di dalam bounds map
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(x, y, 0f);

        // cek tabrakan dengan obstacle layer
        Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.5f, LayerMask.GetMask("Obstacle"));
        if (hit == null) // aman
            return spawnPos;

        tries++;
    }

    // fallback: spawn di dekat player
    return player.position;
}

void SpawnDefault()
{
    if (enemyPrefab == null || player == null) return;

    Vector3 spawnPos = GetValidSpawnPosition();
    Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    currentEnemies++;
}


    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentEnemies < maxEnemies)
        {
            SpawnDefault();
            timer = 0f;
        }
    }

    IEnumerator SpawnOneEnemyWithEffect(Vector2 spawnPos)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.SetActive(false);

        EnemyIMO enemyScript = newEnemy.GetComponent<EnemyIMO>();

        if (enemyScript != null)
            enemyScript.player = player;

        if (spawnEffectFrames != null && spawnEffectFrames.Length > 0)
        {
            GameObject fx = new GameObject("SpawnEffect_IMO");
            fx.transform.position = spawnPos;
            SpriteRenderer fxSr = fx.AddComponent<SpriteRenderer>();
            fxSr.sortingOrder = 20;

            for (int i = 0; i < spawnEffectFrames.Length; i++)
            {
                fxSr.sprite = spawnEffectFrames[i];
                yield return new WaitForSeconds(frameRate);
            }

            Destroy(fx);
        }
        else
        {
            yield return null;
        }

        newEnemy.SetActive(true);
        currentEnemies++;
    }

    // 🔥 Spawn dari salah satu FixedSpawner saat player kebakar
    public void SpawnOnPlayerBurn()
    {
        if (enemyPrefab == null) return;

        // filter hanya spawn point yang valid
        var validPoints = System.Array.FindAll(fixedSpawnPoints, p => p != null);
        if (validPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ Tidak ada FixedSpawner yang ditemukan, spawn di player aja!");
            Instantiate(enemyPrefab, player.position, Quaternion.identity);
            return;
        }

        // pilih 1 titik random
        Transform randomPoint = validPoints[Random.Range(0, validPoints.Length)];
        Instantiate(enemyPrefab, randomPoint.position, Quaternion.identity);

        Debug.Log($"🔥 Enemy spawn di {randomPoint.name} karena player kebakar!");
    }

    public void EnemyDied()
    {
        currentEnemies--;
        if (currentEnemies < 0) currentEnemies = 0;
    }
}
