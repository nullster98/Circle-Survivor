using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    public UpgradeSystem upgr;

    [Header("Active Slots (3)")]
    public TMP_Text[] activeName;   // 3칸
    public TMP_Text[] activeLevel;  // 3칸 ("Lv x/x")

    [Header("Passive Slots (3)")]
    public TMP_Text[] passiveName; 
    public TMP_Text[] passiveLevel;

    void Awake()
    {
        if (!upgr) upgr = FindObjectOfType<UpgradeSystem>(true);
    }

    void OnEnable()
    {
        if (!upgr) upgr = FindObjectOfType<UpgradeSystem>(true);
        if (upgr) upgr.OnUpgradesChanged += Refresh;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Refresh();
    }

    void OnDisable()
    {
        if (upgr) upgr.OnUpgradesChanged -= Refresh;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        if (!upgr) upgr = FindObjectOfType<UpgradeSystem>(true);
        Refresh();
    }

    public void Refresh()
    {
        // 방어
        if (!upgr || upgr.pool == null) { ClearAll(); return; }

        // level>0만 추림
        List<UpgradeState> act = new();
        List<UpgradeState> pas = new();
        foreach (var u in upgr.pool)
        {
            if (u == null || u.level <= 0) continue;
            if (u.category == UpgradeCategory.Active) act.Add(u);
            else pas.Add(u);
        }

        // 최대 3칸만 채움
        Fill(act, activeName, activeLevel);
        Fill(pas, passiveName, passiveLevel);
    }

    void Fill(List<UpgradeState> list, TMP_Text[] nameArr, TMP_Text[] lvlArr)
    {
        for (int i = 0; i < nameArr.Length; i++)
        {
            if (i < list.Count)
            {
                var u = list[i];
                if (nameArr[i]) nameArr[i].text = u.displayName;
                if (lvlArr[i])  lvlArr[i].text  = $"Lv {u.level}/{u.maxLevel}";
            }
            else
            {
                if (nameArr[i]) nameArr[i].text = "-";
                if (lvlArr[i])  lvlArr[i].text  = "";
            }
        }
    }

    void ClearAll()
    {
        Fill(new List<UpgradeState>(), activeName, activeLevel);
        Fill(new List<UpgradeState>(), passiveName, passiveLevel);
    }
}
