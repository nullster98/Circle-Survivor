using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Refs")]
    public Player target;      // Player 드래그

    [Header("HP (오른쪽부터)")]
    public Image[] hearts;     // index 0 = 가장 오른쪽
    public Color aliveColor = Color.red;
    public Color emptyColor = Color.black;

    [Header("XP / Level")]
    public Slider xpSlider;
    public TMP_Text levelText;

    [Header("Timer")]
    public TMP_Text timerText;
    
    public TMP_Text goScore, goKills, goTime, goLevel, goBest;

    void Awake()
    {
        TryBindPlayer();        // 즉시 한 번
    }

    void OnEnable()
    {
        TryBindPlayer();
        Subscribe();
        // 다음 프레임에도 한 번 더(씬 로드 직후 커버)
        StartCoroutine(BindNextFrame());

        // 씬 로드 때마다 재바인딩
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Unsubscribe();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // 새 씬 로드 후 1프레임 뒤 재바인딩
        StartCoroutine(BindNextFrame());
    }

    void Update()
    {
        if (!timerText) return;
        float t = Time.timeSinceLevelLoad;
        int m = (int)(t / 60f);
        int s = (int)(t % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }
    
    void TryBindPlayer()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
    }

    void Subscribe()
    {
        if (target == null) return;
        target.OnHealthChanged -= RefreshHP;  // 중복 방지
        target.OnXPChanged     -= RefreshXP;
        target.OnHealthChanged += RefreshHP;
        target.OnXPChanged     += RefreshXP;

        // 현재 상태로 1회 동기화
        RefreshHP(target.currentHP, target.maxHP);
        RefreshXP(target.currentXP, target.xpToNext, target.level);
    }

    void Unsubscribe()
    {
        if (target == null) return;
        target.OnHealthChanged -= RefreshHP;
        target.OnXPChanged     -= RefreshXP;
    }

    IEnumerator BindNextFrame()
    {
        // 씬 초기 프레임에는 오브젝트 생성 순서가 섞일 수 있어 1프레임 대기
        yield return null;
        TryBindPlayer();
        Subscribe();
    }

    void RefreshHP(int current, int max)
    {
        int offFromRight = max - current; // 오른쪽부터 꺼질 개수
        for (int i = 0; i < hearts.Length; i++)
        {
            bool on = !(i < offFromRight);
            if (hearts[i]) hearts[i].color = on ? aliveColor : emptyColor;
        }
    }

    void RefreshXP(int cur, int req, int level)
    {
        Debug.Log($"UI XP cur={cur} / req={req} (Lv {level})");
        
        if (xpSlider)
        {
            xpSlider.minValue = 0;
            xpSlider.maxValue = Mathf.Max(1, req);
            xpSlider.value    = Mathf.Clamp(cur, 0, req);
        }
        if (levelText) levelText.text = $"Lv. {level}";
    }
    
    public void ShowGameOver(int score, int kills, string timeStr, int level)
    {
        if (goScore) goScore.text = $"Score  {score}";
        if (goKills) goKills.text = $"Kills  {kills}";
        if (goTime)  goTime.text  = $"Time   {timeStr}";
        if (goLevel) goLevel.text = $"Level  {level}";
        if (goBest)  goBest.text  = $"Best   {ScoreManager.Instance.BestScore}";
    }
    public void ResetTimer(){ /* 필요시 내부 타이머 초기화 */ }
}
