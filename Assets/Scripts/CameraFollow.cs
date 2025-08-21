using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 플레이어 Transform
    public Vector2 minBounds;     // 카메라 최소 좌표 (맵 왼쪽 아래)
    public Vector2 maxBounds;     // 카메라 최대 좌표 (맵 오른쪽 위)

    private float halfHeight;
    private float halfWidth;

    void Start()
    {
        Camera cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 플레이어를 중심으로 한 카메라 좌표
        float clampedX = Mathf.Clamp(target.position.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
