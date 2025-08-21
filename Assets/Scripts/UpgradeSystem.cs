using System.Collections.Generic;
using UnityEngine;

public enum UpgradeId
{
    FireRate, BulletSpeed, Damage, MoveSpeed, MaxHP, MagnetPassive,
    OrbitOrbs, SideShot, BigShot
}

public enum UpgradeCategory { Active, Passive}

[System.Serializable]
public class UpgradeState {
    public UpgradeId id;
    public string displayName;
    [TextArea] public string desc;
    public int level = 0;
    public int maxLevel = 5;
    public float weight = 1f; // 등장 가중치
    public UpgradeCategory category = UpgradeCategory.Passive;
}

public class UpgradeSystem : MonoBehaviour
{
    [Header("Pool")]
    public List<UpgradeState> pool = new List<UpgradeState>();

    [Header("Inventory Slots")]
    public int activeSlots = 6;   // 무기 슬롯 (확장 여지)
    public int passiveSlots = 6;  // 패시브 슬롯 (MagnetPassive 같은 것)

    [Header("Refs")]
    public Player player;
    public LevelUpPanel panel;
    ActiveWeapons active;
    
    public event System.Action OnUpgradesChanged;
    
    void Awake()
    {
        if (!player) player = FindObjectOfType<Player>();
        active = player ? player.GetComponent<ActiveWeapons>() : null;
    }

    public List<UpgradeState> GetThreeChoices()
    {
        // 아직 만렙이 아닌 후보만 추출
        List<UpgradeState> cand = pool.FindAll(u => u.level < u.maxLevel);
        // 3개 랜덤(가중치) — 간단히 셔플
        Utils.Shuffle(cand);
        return cand.GetRange(0, Mathf.Min(3, cand.Count));
    }

    public void Apply(UpgradeId id)
    {
        var u = pool.Find(x => x.id == id);
        if (u == null) return;
        if (u.level >= u.maxLevel) return;

        u.level++;
        switch (id)
        {
            case UpgradeId.FireRate:
                player.fireRate = Mathf.Max(0.05f, player.fireRate * 0.9f); break;
            case UpgradeId.BulletSpeed:
                // 총알 프리팹에 기본 속도가 있다면 Bullet.speed를 증가시키도록 설계(간단 예시)
                // 전역에 영향을 주려면 Player에 bulletSpeedMultiplier를 두고 Shoot시 전달
                player.bulletSpeedMul *= 1.15f; break;
            case UpgradeId.Damage:
                player.bulletDamage += 1; break;
            case UpgradeId.MoveSpeed:
                player.moveSpeed *= 1.1f; break;
            case UpgradeId.MaxHP:
                player.maxHP += 1; player.Heal(1); break;
            case UpgradeId.MagnetPassive:
                player.magnetPassive = true; break;
            case UpgradeId.OrbitOrbs:
                if (active)
                {
                    var s = pool.Find(x => x.id == UpgradeId.OrbitOrbs);
                    active.SetOrbitLevel(s.level); // 레벨 = 개수
                }
                break;

            case UpgradeId.SideShot:
                if (active)
                {
                    var s = pool.Find(x => x.id == UpgradeId.SideShot);
                    active.SetSideShotLevel(s.level); // 레벨 = 패턴 단계
                }
                break;

            case UpgradeId.BigShot:
                if (active)
                {
                    var s = pool.Find(x => x.id == UpgradeId.BigShot);
                    active.SetBigShotLevel(s.level); // 레벨↑ = 쿨↓
                }
                break;
        }
        
        OnUpgradesChanged?.Invoke();
    }
}

// 간단 셔플 유틸
public static class Utils
{
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
