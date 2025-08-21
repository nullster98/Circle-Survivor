using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceOrb : MonoBehaviour
{
    public int value = 1;

    // 자석 아이템으로만 사용
    bool magnetOn = false;
    Transform magnetTarget;
    float magnetSpeed = 0f;
    const float pickupDist = 0.2f;

    void Update()
    {
        if (magnetOn && magnetTarget)
        {
            Vector3 dir = (magnetTarget.position - transform.position).normalized;
            transform.Translate(dir * magnetSpeed * Time.deltaTime, Space.World);

            if (Vector2.Distance(transform.position, magnetTarget.position) <= pickupDist)
            {
                // 도착 → 즉시 픽업 처리
                var pl = magnetTarget.GetComponent<Player>();
                if (pl != null) pl.AddXP(value);
                Destroy(gameObject);
            }
        }
    }

    // 자석 아이템이 호출
    public void BeginMagnet(Transform target, float speed)
    {
        magnetOn = true;
        magnetTarget = target;
        magnetSpeed = speed;
    }

    // 평소에는 플레이어가 직접 닿아야 먹힘
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pl = other.GetComponent<Player>();
        if (pl != null) pl.AddXP(value);
        Destroy(gameObject);
    }
}
