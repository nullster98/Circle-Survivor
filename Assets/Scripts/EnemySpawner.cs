using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Bounds")]
    public BoxCollider2D mapBounds; // 맵 영역(카메라 클램프에도 쓰던 그것)
    public float margin = 1.5f;     // 맵 밖 얼마나 떨어진 곳에서 스폰할지

    [Header("Prefabs")]
    public GameObject gruntPrefab;
    public GameObject runnerPrefab;
    public GameObject bossPrefab;
    
    [Header("Spawn Curve")]
    public AnimationCurve spawnIntervalByTime = AnimationCurve.Linear(0, 1.2f, 180f, 0.35f); // t초→간격
    public float runnerRatioStart = 0.2f;
    public float runnerRatioEnd   = 0.6f;
    public float bossAt = 120f;
    bool bossSpawned = false;

    private Transform player;

    IEnumerator Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        while (true)
        {
            float t = Time.timeSinceLevelLoad;
            float interval = Mathf.Clamp(spawnIntervalByTime.Evaluate(t), 0.1f, 5f);

            if (!bossSpawned && t >= bossAt)
            {
                bossSpawned = true;
                Instantiate(bossPrefab, GetRandomOutsidePoint(), Quaternion.identity);
            }

            float rRatio = Mathf.Lerp(runnerRatioStart, runnerRatioEnd, Mathf.Clamp01(t/180f));
            GameObject prefab = (Random.value < rRatio) ? runnerPrefab : gruntPrefab;
            Instantiate(prefab, GetRandomOutsidePoint(), Quaternion.identity);

            yield return new WaitForSeconds(interval);
        }
    }

    Vector2 GetRandomOutsidePoint()
    {
        Bounds b = mapBounds.bounds;
        float left   = b.min.x - margin;
        float right  = b.max.x + margin;
        float bottom = b.min.y - margin;
        float top    = b.max.y + margin;

        // 사각형 둘레에서 랜덤
        int side = Random.Range(0, 4); // 0=좌,1=우,2=하,3=상
        switch (side)
        {
            case 0: return new Vector2(left, Random.Range(bottom, top));
            case 1: return new Vector2(right, Random.Range(bottom, top));
            case 2: return new Vector2(Random.Range(left, right), bottom);
            default: return new Vector2(Random.Range(left, right), top);
        }
    }
}