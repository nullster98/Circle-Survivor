using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    // ===== Movement =====
    [Header("Move")]
    public float moveSpeed = 5f;
    Vector2 moveDir;

    // ===== Shooting =====
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;          // Player의 자식 빈 오브젝트
    public float firePointDistance = 0.5f;
    public float fireRate = 0.3f;
    float nextFireTime = 0f;
    Vector2 shootDir = Vector2.right;    // 마지막 이동/발사 방향
    
    public float bulletSpeedMul = 1f; 
    public int bulletDamage = 1; 
    public bool magnetPassive = false;

    // ===== HP / i-Frame =====
    [Header("HP")]
    public int maxHP = 3;
    [HideInInspector] public int currentHP;

    [Header("I-Frame")]
    public float iFrameDuration = 2f;
    public float blinkInterval = 0.1f;

    public event Action<int,int> OnHealthChanged; // (current, max)
    public event Action OnDead;

    float invulnUntil = 0f;
    SpriteRenderer[] sprites;

    // ===== Level / XP =====
    [Header("Level / XP")]
    public int level = 1;
    public int currentXP = 0;
    public int baseXP = 10;
    public float growth = 1.35f; // xpToNext ≈ baseXP * growth^(level-1)
    public int xpToNext = 10;    // 시작 요구치 (Start에서 초기화)

    public event Action<int,int,int> OnXPChanged; // (cur, req, level)
    public event Action<int> OnLevelUp;

    void Awake()
    {
        // HP init
        currentHP = maxHP;
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        OnHealthChanged?.Invoke(currentHP, maxHP);

        // XP init
        RecalcXPToNext();
        OnXPChanged?.Invoke(currentXP, xpToNext, level);
    }

    void Update()
    {
        // === 이동 입력 (WASD) ===
        moveDir = new Vector2(
            (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)
        );
        if (moveDir != Vector2.zero)
        {
            moveDir = moveDir.normalized;
            shootDir = moveDir; // 마지막 이동방향 = 발사 방향
        }

        // 위치 이동
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        // FirePoint를 방향에 맞춰 이동
        if (firePoint != null) firePoint.localPosition = shootDir * firePointDistance;

        // 자동 발사
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var b = go.GetComponent<Bullet>();
        if (b != null) { 
            b.SetDirection(shootDir); 
            b.damage = bulletDamage; 
            b.speedMul = bulletSpeedMul; 
        }

        // 플레이어와 총알 충돌 무시
        var bulletCol = go.GetComponent<Collider2D>();
        var playerCol = GetComponent<Collider2D>();
        if (bulletCol && playerCol) Physics2D.IgnoreCollision(bulletCol, playerCol);
    }

    // ===== HP / Damage =====
    public void TakeDamage(int amount)
    {
        if (Time.time < invulnUntil) return;

        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            OnDead?.Invoke();
            Destroy(gameObject);
            return;
        }

        invulnUntil = Time.time + iFrameDuration;
        StopAllCoroutines();
        StartCoroutine(BlinkIFrames());
    }

    IEnumerator BlinkIFrames()
    {
        while (Time.time < invulnUntil)
        {
            SetSpritesVisible(false);
            yield return new WaitForSeconds(blinkInterval);
            SetSpritesVisible(true);
            yield return new WaitForSeconds(blinkInterval);
        }
        SetSpritesVisible(true);
    }

    void SetSpritesVisible(bool visible)
    {
        if (sprites == null || sprites.Length == 0)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = visible;
            return;
        }
        foreach (var s in sprites) if (s) s.enabled = visible;
    }

    // 적 접촉 피해 (i-Frame이 쿨다운처럼 막아줌)
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            int dmg = 1;
            var enemy = other.GetComponent<EnemyBase>();
            if (enemy) dmg = Mathf.Max(1, enemy.contactDamage);
            TakeDamage(dmg);
        }
    }

    // ===== XP / Level =====
    public void AddXP(int amount)
    {
        currentXP += Mathf.Max(0, amount);

        // 레벨업을 여러 번 할 수 있으므로 단계별로 처리
        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;
            RecalcXPToNext();

            // 1) 먼저 UI에 새 통/현재치 동기화 (리셋 보장)
            OnXPChanged?.Invoke(currentXP, xpToNext, level);

            // 2) 그 다음 레벨업 이벤트 (패널 오픈/일시정지)
            OnLevelUp?.Invoke(level);
        }

        // 루프 종료 후 최종 상태를 한 번 더 브로드캐스트(잔여 XP 표시)
        OnXPChanged?.Invoke(currentXP, xpToNext, level);
    }

    void RecalcXPToNext()
    {
        xpToNext = Mathf.Max(5, Mathf.RoundToInt(baseXP * Mathf.Pow(growth, level - 1)));
    }
    
    // ===== Heal =====
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        int before = currentHP;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        if (currentHP != before)
            OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}
