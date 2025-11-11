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

    [Header("Fixed Spawn Points (untuk player burn)")]
    public Transform[] fixedSpawners; // isi dengan FixedSpawner (1)-(8)

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
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.SetActive(false);

        EnemyJESTER enemyScript = newEnemy.GetComponent<EnemyJESTER>();
        if (enemyScript != null)
            enemyScript.player = player;

        if (spawnEffectFrames != null && spawnEffectFrames.Length > 0)
        {
            GameObject fx = new GameObject("SpawnEffect_Jester");
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

    public void EnemyDied()
    {
        currentEnemies--;
        if (currentEnemies < 0) currentEnemies = 0;
    }

    // 🧨 dipanggil pas player kebakar
    // 🃏 Spawn langsung saat player kebakar (tanpa ganggu spawner utama)
public void SpawnOnPlayerBurn()
{
    if (enemyPrefab == null || player == null) return;

    // pilih posisi spawn random dari 6 titik FixedSpawner
    GameObject[] fixedSpawners = GameObject.FindGameObjectsWithTag("FixedSpawner");
    if (fixedSpawners.Length > 0)
    {
        int randomIndex = Random.Range(0, fixedSpawners.Length);
        Vector2 spawnPos = fixedSpawners[randomIndex].transform.position;
        StartCoroutine(SpawnOneEnemyWithEffect(spawnPos));
    }
    else
    {
        // fallback kalau gak ada FixedSpawner, spawn di sekitar player
        Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle * spawnRadius;
        StartCoroutine(SpawnOneEnemyWithEffect(spawnPos));
    }

    Debug.Log("🎭 JESTER spawn karena player kebakar!");
}

}
