using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGrunt : EnemyBase
{
    void Reset() { moveSpeed = 1.8f; maxHP = 6; contactDamage = 1; }
}

public class EnemyRunner : EnemyBase
{
    void Reset() { moveSpeed = 3.2f; maxHP = 3; contactDamage = 1; }
}

[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    public bool isBoss = false;
    
    [Header("Drop: Experience Orb")]
    public GameObject xpOrbPrefab;
    public int xpValue = 1;
    
    [Header("Drop: Items (exclusive)")]
    [Range(0f, 1f)] public float itemDropChance = 0.30f;  // 아이템 드랍 시도 확률(전체)
    [Range(0f, 1f)] public float magnetWeight = 0.5f;     // 자석:힐 비율(합 1.0 권장)
    [Range(0f, 1f)] public float healWeight = 0.5f;       // ex) 0.7 / 0.3 등 가중치
    public GameObject magnetItemPrefab;                   // PickupItem[type=MagnetAllXP]
    public GameObject healItemPrefab;                     // PickupItem[type=Heal1]
    
    public float moveSpeed = 2f;
    public int maxHP = 5;
    public int contactDamage = 1;
    
    [Header("Separation")]
    public float separationRadius = 0.6f;   // 이 거리 안에서는 서로 떨어지려 함
    public float separationForce  = 1.5f;   // 밀어내기 강도
    public float avoidPlayerDist  = 0.45f;  // 플레이어와 최소 거리

    protected Transform player;
    protected int currentHP;

    void Awake()
    {
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        OnUpdate();
    }

    // 자식에서 호출할 수 있는 가상 메서드
    protected virtual void OnUpdate()
    {
        if (player == null) return;

        // 기본 추적
        Vector2 dir = (player.position - transform.position).normalized;

        // --- 분리력 계산 ---
        Vector2 sep = Vector2.zero;

        // 1) 다른 적들로부터 분리
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int mask = 1 << enemyLayer;
        var hits = Physics2D.OverlapCircleAll(transform.position, separationRadius, mask);
        foreach (var h in hits)
        {
            if (h.attachedRigidbody == null || h.attachedRigidbody.gameObject == gameObject) continue;
            Vector2 toMe = (Vector2)(transform.position - h.transform.position);
            float d = toMe.magnitude;
            if (d > 0.0001f)
                sep += toMe / (d * d); // 가까울수록 강하게 밀어냄
        }

        // 2) 플레이어와 너무 겹치지 않기(살짝 밀어냄)
        Vector2 toPlayer = (Vector2)(transform.position - player.position);
        float dp = toPlayer.magnitude;
        if (dp < avoidPlayerDist && dp > 0.0001f)
        {
            sep += toPlayer.normalized * ( (avoidPlayerDist - dp) / avoidPlayerDist );
        }

        // 분리력 가중 합성
        Vector2 finalDir = (dir + sep * separationForce).normalized;

        transform.Translate(finalDir * moveSpeed * Time.deltaTime, Space.World);
    }

    public virtual void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        ScoreManager.Instance?.OnEnemyKilled(isBoss);
        
        if (xpOrbPrefab != null)
        {
            var orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
            var xp = orb.GetComponent<ExperienceOrb>();
            if (xp != null) xp.value = Mathf.Max(1, xpValue);
        }
        
        if (Random.value <= itemDropChance)
        {
            // 가중치 정규화(인스펙터 합계가 꼭 1이 아닐 수 있으니 안전하게)
            float sum = Mathf.Max(0.0001f, magnetWeight + healWeight);
            float r = Random.value * sum;

            GameObject prefabToDrop = (r < magnetWeight) ? magnetItemPrefab : healItemPrefab;

            if (prefabToDrop != null)
                Instantiate(prefabToDrop, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var dmg = other.GetComponent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(contactDamage);
        }
    }
}