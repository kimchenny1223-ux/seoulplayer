using UnityEngine;
<<<<<<< HEAD
using UnityEngine.SceneManagement; // 화면 전환을 위해 꼭 필요해요!
=======
>>>>>>> af82c2242aaf77d03e7cdea47da87e9ff6880e13
using TMPro; // TMP 텍스트를 쓰기 위해 꼭 필요합니다!

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Score System")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Timer System")]
    public float timeRemaining = 90f; // 1분 30초 = 90초
<<<<<<< HEAD
    public TextMeshProUGUI timerText;
=======
    public TextMeshProUGUI timerText; // 화면에 시간을 보여줄 텍스트 칸
>>>>>>> af82c2242aaf77d03e7cdea47da87e9ff6880e13
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
<<<<<<< HEAD
                timeRemaining = 0;
=======
                // 시간이 0 이하가 되면 게임 종료!
                timeRemaining = 0;
                isGameOver = true;
>>>>>>> af82c2242aaf77d03e7cdea47da87e9ff6880e13
                GameOver();
            }
        }
    }

<<<<<<< HEAD
    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (timerText != null) timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining).ToString();
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
=======
    public void AddScore(int amount)
    {
        if (isGameOver) return; // 게임이 끝났으면 점수가 안 오르게 막음
>>>>>>> af82c2242aaf77d03e7cdea47da87e9ff6880e13
        score += amount;
        UpdateUI();
    }

    public void DeductScore(int amount)
    {
<<<<<<< HEAD
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
=======
        if (isGameOver) return; // 게임이 끝났으면 점수가 안 깎이게 막음
        score -= amount;
        if (score < 0) score = 0; 
        UpdateUI();
    }

    void UpdateUI()
    {
        // 점수 글자 업데이트
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }

        // 시간 글자 업데이트 (분:초 형태로 이쁘게 출력)
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("Time: {0:0}:{1:00}", minutes, seconds);
        }
    }

    // 시간이 다 되었을 때 실행되는 함수
    void GameOver()
    {
        if (timerText != null)
        {
            timerText.text = "TIME UP!!";
        }
        
        // 게임 안의 모든 사물(시간)을 일시정지 시킵니다.
        Time.timeScale = 0f; 
        
        Debug.Log("게임 종료! 최종 점수: " + score);
    }
>>>>>>> af82c2242aaf77d03e7cdea47da87e9ff6880e13
}