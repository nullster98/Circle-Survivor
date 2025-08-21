using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Kills { get; private set; }
    public int Score { get; private set; }
    public int BestScore { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BestScore = PlayerPrefs.GetInt("BEST_SCORE", 0);
    }

    public void ResetRun()
    {
        Kills = 0;
        Score = 0;
    }

    public void OnEnemyKilled(bool isBoss)
    {
        Kills++;
        Score += isBoss ? 100 : 10;
    }

    public void AddTimeBonus(int seconds)
    {
        Score += seconds; // 생존 1초=1점 정도
    }

    public void TryCommitBest(float elapsedSeconds)
    {
        AddTimeBonus(Mathf.RoundToInt(elapsedSeconds));
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt("BEST_SCORE", BestScore);
        }
    }
}