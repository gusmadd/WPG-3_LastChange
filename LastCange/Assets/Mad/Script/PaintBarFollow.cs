using UnityEngine;

public class PainBarFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 1.5f, 0); // tinggi di atas kepala

    void LateUpdate()
    {
        if (player != null)
            transform.position = player.position + offset;
    }
}
