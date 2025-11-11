using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // drag Player ke sini
    public float smoothSpeed = 0.125f; // kelancaran kamera
    private Camera cam;
    private float minX, maxX, minY, maxY;

    [Header("Zoom Effect")]
    public bool enableIntroZoom = true;
    public float startSize = 10f;       // ukuran awal (zoom out)
    public float targetSize = 5f;       // ukuran akhir (zoom in)
    public float zoomDuration = 2f;     // lama efek zoom

    void Start()
    {
        cam = GetComponent<Camera>();

        // Set awal zoom out
        if (enableIntroZoom)
            cam.orthographicSize = startSize;

        // Jalankan efek zoom
        if (enableIntroZoom)
            StartCoroutine(ZoomInEffect());

        // Ambil ukuran map otomatis dari SpriteRenderer
        SpriteRenderer map = GameObject.Find("Map").GetComponent<SpriteRenderer>();
        float mapWidth = map.bounds.size.x;
        float mapHeight = map.bounds.size.y;

        minX = map.bounds.min.x;
        maxX = map.bounds.max.x;
        minY = map.bounds.min.y;
        maxY = map.bounds.max.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 🔧 Update ukuran kamera setiap frame agar clamp akurat
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 desiredPos = new Vector3(player.position.x, player.position.y, transform.position.z);

        float clampedX = Mathf.Clamp(desiredPos.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(desiredPos.y, minY + halfHeight, maxY - halfHeight);

        Vector3 clampedPos = new Vector3(clampedX, clampedY, desiredPos.z);

        transform.position = Vector3.Lerp(transform.position, clampedPos, smoothSpeed);
    }

    IEnumerator ZoomInEffect()
    {
        float elapsed = 0f;
        float initialSize = startSize;

        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(initialSize, targetSize, elapsed / zoomDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }
}