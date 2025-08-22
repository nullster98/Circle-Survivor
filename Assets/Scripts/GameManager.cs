using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; using UnityEngine.UI; // (필요시)

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State { Title, Playing, LevelUpPause, GameOver }
    public State Current { get; private set; } = State.Title;

    public static bool StartOnLoad = false;

    [Header("UI Canvases")]
    public GameObject titleCanvas;
    public GameObject levelUpCanvas;
    public GameObject gameOverCanvas;

    [Header("Refs")]
    public Player player;
    public UIManager ui;
    public LevelUpPanel levelUpPanel;
    public AudioSource bgm;

    void Awake()
    {
        Instance = this;

        Time.timeScale = 0f;

        // 🔹 즉시 한 번 시도
        RebindImmediate();

        // 🔹 다음 프레임 한 번 더(씬 로드 직후 생성되는 오브젝트까지 커버)
        StartCoroutine(RebindNextFrame());
        
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;   // ✅ 구독
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;   // ✅ 해제 (Restart/Title 시 꼭 호출됨)
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬에서 새 GM이든 기존 GM이든 여기서 한 번만 리바인딩
        StartCoroutine(RebindNextFrame());
    }

    void RebindImmediate()
    {
        // Canvas 그룹
        if (!titleCanvas || !levelUpCanvas || !gameOverCanvas)
        {
            var rootCanvas = GameObject.Find("Canvas");
            if (rootCanvas)
            {
                if (!titleCanvas)   titleCanvas   = FindChild(rootCanvas.transform, "TitleCanvas")?.gameObject;
                if (!levelUpCanvas) levelUpCanvas = FindChild(rootCanvas.transform, "LevelupCanvas")?.gameObject;
                if (!gameOverCanvas)gameOverCanvas= FindChild(rootCanvas.transform, "GameOverCanvas")?.gameObject;
            }
        }

        // Player / UI / LevelUpPanel
        if (!player)        player        = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (!ui)            ui            = FindObjectOfType<UIManager>(true);
        if (!levelUpPanel)  levelUpPanel  = FindObjectOfType<LevelUpPanel>(true);

        // 이벤트 중복 방지 후 재구독
        if (player != null)
        {
            player.OnLevelUp -= OnPlayerLevelUp;
            player.OnDead    -= OnPlayerDead;
            player.OnLevelUp += OnPlayerLevelUp;
            player.OnDead    += OnPlayerDead;
        }

        // 타이틀 상태 반영
        SwitchUI(State.Title);
    }

    System.Collections.IEnumerator RebindNextFrame()
    {
        yield return null;
        RebindImmediate();  // 다음 프레임에도 동일 로직 수행
        if (StartOnLoad) { StartOnLoad = false; Btn_StartGame(); }
    }

    // 트랜스폼 자식 이름으로 찾기
    Transform FindChild(Transform root, string name)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            var t = root.GetChild(i);
            if (t.name == name) return t;
        }
        return null;
    }

    void SetGroup(GameObject go, bool on)
    {
        if (!go) return;
        go.SetActive(true);
        var cg = go.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = on ? 1f : 0f;
            cg.interactable = on;
            cg.blocksRaycasts = on;
        }
        else
        {
            go.SetActive(on);
        }
    }

    void SwitchUI(State s)
    {
        Current = s;
        SetGroup(titleCanvas,    s == State.Title);
        SetGroup(levelUpCanvas,  s == State.LevelUpPause);
        SetGroup(gameOverCanvas, s == State.GameOver);
    }

    // === 버튼에서 호출 ===
    public void Btn_StartGame()
    {
        if (ScoreManager.Instance == null)
            new GameObject("ScoreManager").AddComponent<ScoreManager>();

        Debug.Log("StartGame: timescale=1, state=Playing");
        Time.timeScale = 1f;
        SwitchUI(State.Playing);
        ScoreManager.Instance.ResetRun();
        ui?.ResetTimer();
        bgm.Play();
    }

    public void Btn_Restart()
    {
        StartOnLoad = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Btn_Title()
    {
        StartOnLoad = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnPlayerLevelUp(int newLv)
    {
        if (Current != State.Playing) return;
        Time.timeScale = 0f;
        SwitchUI(State.LevelUpPause);
        levelUpPanel?.Open();
    }

    void OnPlayerDead()
    {
        if (Current == State.GameOver) return;
        Time.timeScale = 0f;
        SwitchUI(State.GameOver);

        float t = Time.timeSinceLevelLoad;
        int m = (int)(t / 60f);
        int s = (int)(t % 60f);
        string timeStr = $"{m:00}:{s:00}";

        ui?.ShowGameOver(ScoreManager.Instance.Score, ScoreManager.Instance.Kills, timeStr, player?.level ?? 1);
        ScoreManager.Instance.TryCommitBest(t);
        bgm.Stop();
    }

    public void ResumeFromLevelUp()
    {
        if (Current != State.LevelUpPause) return;
        Time.timeScale = 1f;
        SwitchUI(State.Playing);
    }
}
