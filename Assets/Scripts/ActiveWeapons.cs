using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWeapons : MonoBehaviour
{
    [Header("Common")]
    public Transform firePoint;          // 없으면 transform 사용
    public GameObject bulletPrefab;      // 일반 탄(기존 프리팹)
    public GameObject bigBulletPrefab;   // 큰 탄(새 프리팹)
    public LayerMask enemyLayer;

    [Header("Orbit (주변 회전탄)")]
    public GameObject orbitOrbPrefab;    // 작은 오브(Trigger + OrbitingOrb)
    public float orbitRadius = 1.2f;
    public float orbitRotateSpeed = 120f; // deg/sec
    public int orbitLevel = 0;

    [Header("Side Shot (양옆 + 방향 증가)")]
    public float sideShotInterval = 0.6f;
    public int sideShotLevel = 0;
    float sideShotTimer = 0f;

    [Header("Big Shot (주기적 큰 탄)")]
    public float bigShotBaseCooldown = 4f;
    public int bigShotLevel = 0;
    Coroutine bigShotCo;

    // 내부
    Transform orbitRoot;
    readonly List<GameObject> orbiters = new();

    // (옵션) Player 스탯 반영
    Player player;

    void Awake()
    {
        player = GetComponent<Player>();
        if (!firePoint) firePoint = transform;
        SetupOrbitRoot();
        RebuildOrbiters();
        RestartBigShot();
    }

    void Update()
    {
        RotateOrbit();

        if (sideShotLevel > 0)
        {
            sideShotTimer -= Time.deltaTime;
            if (sideShotTimer <= 0f)
            {
                FireSidePattern();
                sideShotTimer = Mathf.Max(0.1f, sideShotInterval);
            }
        }
    }

    // ---------- Orbit ----------
    void SetupOrbitRoot()
    {
        if (orbitRoot == null)
        {
            var go = new GameObject("OrbitRoot");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            orbitRoot = go.transform;
        }
    }

    public void RebuildOrbiters()
    {
        // 기존 제거
        foreach (var o in orbiters) if (o) Destroy(o);
        orbiters.Clear();

        if (orbitLevel <= 0 || orbitOrbPrefab == null) return;

        int n = orbitLevel; // 레벨=개수
        for (int i = 0; i < n; i++)
        {
            float ang = (360f / n) * i * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * orbitRadius;

            var orb = Instantiate(orbitOrbPrefab, orbitRoot);
            orb.transform.localPosition = pos;

            // 데미지/틱 인터벌을 Player 스탯에 맞추고 싶으면 여기서 세팅
            var dmg = orb.GetComponent<OrbitingOrb>();
            if (dmg && player != null)
            {
                dmg.damage = Mathf.Max(1, player.bulletDamage);
            }

            orbiters.Add(orb);
        }
    }

    void RotateOrbit()
    {
        if (orbitRoot && orbitLevel > 0)
            orbitRoot.Rotate(0f, 0f, orbitRotateSpeed * Time.deltaTime);
    }

    public void SetOrbitLevel(int lvl)
    {
        orbitLevel = Mathf.Clamp(lvl, 0, 8);
        RebuildOrbiters();
    }

    // ---------- Side Shot ----------
    void FireSidePattern()
    {
        if (!bulletPrefab) return;

        // 레벨별 각도 집합(도 단위, +X축 기준)
        // L1: 좌/우(±90)
        // L2: + 정면(0)
        // L3: + 후면(180)
        // L4: + 대각(±45)
        // L5+: + 대각(±135)
        var angles = GetSideAngles(sideShotLevel);

        foreach (float deg in angles)
        {
            FireBulletAtAngle(deg, false);
        }
    }

    List<float> GetSideAngles(int lvl)
    {
        var set = new HashSet<float>();
        if (lvl >= 1) { set.Add(90); set.Add(-90); }
        if (lvl >= 2) { set.Add(0); }
        if (lvl >= 3) { set.Add(180); }
        if (lvl >= 4) { set.Add(45); set.Add(-45); }
        if (lvl >= 5) { set.Add(135); set.Add(-135); }
        // 필요하면 더 추가 가능
        return new List<float>(set);
    }

    void FireBulletAtAngle(float deg, bool isBig)
    {
        var prefab = isBig ? bigBulletPrefab : bulletPrefab;
        if (!prefab) return;

        Vector2 dir = new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad)).normalized;
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);
        var b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.SetDirection(dir);

            // Player 스탯 반영
            if (player != null)
            {
                b.damage = isBig ? Mathf.Max(2, player.bulletDamage * 3) : player.bulletDamage;
                b.speedMul = player.bulletSpeedMul;
            }

            // 플레이어와 충돌 무시
            var bulletCol = go.GetComponent<Collider2D>();
            var playerCol = GetComponent<Collider2D>();
            if (bulletCol && playerCol) Physics2D.IgnoreCollision(bulletCol, playerCol);
        }
    }

    public void SetSideShotLevel(int lvl)
    {
        sideShotLevel = Mathf.Clamp(lvl, 0, 6);
        // 필요하면 발사 간격도 레벨에 따라 조정 가능
    }

    // ---------- Big Shot ----------
    IEnumerator BigShotLoop()
    {
        while (bigShotLevel > 0)
        {
            FireBigBullet();
            float cd = Mathf.Max(0.4f, bigShotBaseCooldown * Mathf.Pow(0.85f, bigShotLevel - 1));
            yield return new WaitForSeconds(cd);
        }
        bigShotCo = null;
    }

    void FireBigBullet()
    {
        // 가장 가까운 적 방향, 없으면 +X
        Vector2 dir = FindDirectionToNearestEnemyOrRight();
        float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        FireBulletAtAngle(deg, true);
    }

    Vector2 FindDirectionToNearestEnemyOrRight()
    {
        float best = float.MaxValue;
        Vector2 dir = Vector2.right;

        var all = FindObjectsOfType<EnemyBase>();
        if (all != null && all.Length > 0)
        {
            Vector2 my = transform.position;
            foreach (var e in all)
            {
                if (!e) continue;
                float d = Vector2.SqrMagnitude((Vector2)e.transform.position - my);
                if (d < best) { best = d; dir = ((Vector2)e.transform.position - my).normalized; }
            }
        }
        return dir;
    }

    public void SetBigShotLevel(int lvl)
    {
        bigShotLevel = Mathf.Clamp(lvl, 0, 10);
        RestartBigShot();
    }

    void RestartBigShot()
    {
        if (bigShotCo != null) StopCoroutine(bigShotCo);
        if (bigShotLevel > 0) bigShotCo = StartCoroutine(BigShotLoop());
    }
}
