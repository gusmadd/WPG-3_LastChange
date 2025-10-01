using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // drag Player ke sini
    public float smoothSpeed = 0.125f; // kelancaran kamera
    private Camera cam;
    private float halfHeight;
    private float halfWidth;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        cam = GetComponent<Camera>();
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

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

        Vector3 desiredPos = new Vector3(player.position.x, player.position.y, transform.position.z);

        float clampedX = Mathf.Clamp(desiredPos.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(desiredPos.y, minY + halfHeight, maxY - halfHeight);

        Vector3 clampedPos = new Vector3(clampedX, clampedY, desiredPos.z);

        transform.position = Vector3.Lerp(transform.position, clampedPos, smoothSpeed);
    }
}
