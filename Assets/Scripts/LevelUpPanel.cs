using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpPanel : MonoBehaviour
{
    public UpgradeSystem upgr;
    public Button[] buttons;       // 3개 버튼
    public TMP_Text[] nameTexts;   // 각 버튼 제목
    public TMP_Text[] descTexts;   // 각 버튼 설명

    List<UpgradeState> current;

    void Awake()
    {
        // 씬 리로드로 참조가 비었을 때 자동 복구
        if (upgr == null) upgr = FindObjectOfType<UpgradeSystem>(includeInactive: true);
    }

    public void Open()
    {
        // *** 널 가드 ***
        if (upgr == null) { 
            upgr = FindObjectOfType<UpgradeSystem>(includeInactive: true);
            if (upgr == null) { Debug.LogError("LevelUpPanel: UpgradeSystem missing"); CloseResume(); return; }
        }
        if (buttons == null || nameTexts == null || descTexts == null ||
            buttons.Length < 3 || nameTexts.Length < 3 || descTexts.Length < 3)
        { Debug.LogError("LevelUpPanel: UI arrays not wired"); CloseResume(); return; }

        gameObject.SetActive(true);

        var choices = upgr.GetThreeChoices();
        if (choices == null || choices.Count == 0) { 
            Debug.LogWarning("LevelUpPanel: no choices (all maxed?)"); 
            CloseResume(); 
            return; 
        }

        current = choices;

        for (int i = 0; i < buttons.Length; i++)
        {
            bool on = i < current.Count && buttons[i] != null;
            if (buttons[i]) buttons[i].gameObject.SetActive(on);
            if (!on) continue;

            var u = current[i];
            if (nameTexts[i]) nameTexts[i].text = $"{u.displayName}  Lv.{u.level+1}/{u.maxLevel}";
            if (descTexts[i]) descTexts[i].text = u.desc;

            int idx = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => Choose(idx));
        }

        EventSystem.current?.SetSelectedGameObject(buttons[0]?.gameObject);
    }

    void Choose(int idx)
    {
        if (current == null || idx < 0 || idx >= current.Count) { CloseResume(); return; }
        if (upgr == null) upgr = FindObjectOfType<UpgradeSystem>(includeInactive: true);
        if (upgr == null) { Debug.LogError("LevelUpPanel: UpgradeSystem missing on Choose"); CloseResume(); return; }

        upgr.Apply(current[idx].id);
        CloseResume();
    }

    void CloseResume()
    {
        EventSystem.current?.SetSelectedGameObject(null);
        gameObject.SetActive(false);
        GameManager.Instance?.ResumeFromLevelUp();
    }
}