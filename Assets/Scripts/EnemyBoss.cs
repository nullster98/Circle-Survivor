using UnityEngine;

public class EnemyBoss : EnemyBase
{
    public float dashInterval = 4f;
    public float dashMultiplier = 2.2f;
    private float nextDash;

    void Reset()
    {
        moveSpeed = 1.6f;
        maxHP = 120;
        contactDamage = 3;
    }

    void Start()
    {
        nextDash = Time.time + dashInterval;
    }

    protected override void OnUpdate()
    {
        // 부모 이동 로직 유지
        base.OnUpdate();

        if (Time.time >= nextDash)
        {
            nextDash = Time.time + dashInterval;
            StartCoroutine(DashBurst());
        }
    }

    System.Collections.IEnumerator DashBurst()
    {
        float t = 0.7f;
        float end = Time.time + t;
        while (Time.time < end)
        {
            if (player == null) yield break;
            Vector2 dir = (player.position - transform.position).normalized;
            transform.Translate(dir * moveSpeed * dashMultiplier * Time.deltaTime, Space.World);
            yield return null;
        }
    }

    protected override void Die()
    {
        base.Die();
    }
}