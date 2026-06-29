using UnityEngine;
using UnityEngine.SceneManagement; // 화면 전환을 위해 꼭 필요해요!
using TMPro; // TMP 텍스트를 쓰기 위해 꼭 필요합니다!

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Score System")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Timer System")]
    public float timeRemaining = 90f; // 1분 30초 = 90초
    public TextMeshProUGUI timerText;
    private bool isGameOver = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // 게임 오버 상태가 아니라면 시간이 계속 줄어듭니다.
        if (!isGameOver)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateUI();
            }
            else
            {
                timeRemaining = 0;
                GameOver();
            }
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (timerText != null) timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining).ToString();
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        UpdateUI();
    }

    public void DeductScore(int amount)
    {
        if (isGameOver) return;
        score -= amount;
        if (score < 0) score = 0;
        UpdateUI();
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
    }

    // ==========================================
    // 버튼을 누르면 게임이 시작되는(화면이 넘어가는) 함수입니다!
    public void SceneChange()
    {
        // "GameScene" 자리에 실제 플레이하는 게임 씬(화면) 이름을 정확하게 적어주세요.
        // 만약 실제 게임 화면 이름이 'tower63' 이라면 "tower63"으로 적어야 합니다!
        SceneManager.LoadScene("tower63"); 
    }
    // ==========================================
}