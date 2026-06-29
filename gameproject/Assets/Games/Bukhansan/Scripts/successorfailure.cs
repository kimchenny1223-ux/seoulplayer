using UnityEngine;
using TMPro;

public class successorfailure : MonoBehaviour
{
    [Header("참조할 외부 클래스")]
    public PlayerHP playerHP;       
    public GameTimer gameTimer;     

    [Header("참조할 성공/실패 스프라이트")]
    public GameObject successObject; 
    public GameObject failObject;    

    [Header("점수 표시 컴포넌트")]
    // 성공/실패 시 공용으로 쓰거나 각각 배치할 수 있도록 텍스트 슬롯을 분리하거나 하나로 씁니다.
    // 여기서는 실패 스프라이트 자식으로 들어갈 텍스트 슬롯도 만들어 두었습니다.
    public TextMeshPro scoreText;      // 성공용 텍스트
    public TextMeshPro failScoreText;  // 실패용 텍스트 (필요 없다면 인스펙터에서 비워둬도 됨)

    [Header("등반 설정")]
    public float goalYPosition = 100f; 
    public Vector3 objectOffset = new Vector3(0f, 2f, 0f); 

    private bool isGameOver = false;

    void Start()
    {
        if (successObject != null) successObject.SetActive(false);
        if (failObject != null) failObject.SetActive(false);

        if (gameTimer != null)
        {
            //gameTimer.StartTimer();
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExitGame();
            }
            return;
        }

        if (transform.position.y >= goalYPosition)
        {
            SetSuccess();
        }

        if (playerHP != null && playerHP.hp <= 0)
        {
            SetFail();
        }
    }

    void SetSuccess()
    {
        isGameOver = true;
        Debug.Log("🎉 등반 성공!");

        float clearTime = 0f;
        if (gameTimer != null)
        {
            gameTimer.StopTimer();
            clearTime = gameTimer.GetElapsedTime();
        }

        int baseScore = 50000; 
        int hpBonus = 0;       
        int timeBonus = 0;     

        if (playerHP != null)
        {
            hpBonus = playerHP.hp * 10000; 
        }

        timeBonus = Mathf.Max(0, 50000 - (int)(clearTime * 100));
        int finalScore = baseScore + hpBonus + timeBonus;

        if (successObject != null)
        {
            successObject.transform.position = transform.position + objectOffset;
            successObject.SetActive(true);
        }

        if (scoreText != null)
        {
            int minutes = Mathf.FloorToInt(clearTime / 60f);
            int seconds = Mathf.FloorToInt(clearTime % 60f);

            scoreText.text = $"🎯 등반 성공!\n" +
                             $"⏱️ 등반 시간: {minutes:00}:{seconds:00}\n" +
                             $"🎁 시간 보너스: {timeBonus:N0}점\n" +
                             $"--------------------\n" +
                             $"TOTAL SCORE: {finalScore:N0}";
        }

        Time.timeScale = 0f;
    }

    void SetFail()
    {
        isGameOver = true;
        Debug.Log("💀 등반 실패... (HP 0 감지됨)");

        // 1. 실패하는 순간에도 타이머를 멈추고 기록을 가져옵니다.
        float clearTime = 0f;
        if (gameTimer != null)
        {
            gameTimer.StopTimer();
            clearTime = gameTimer.GetElapsedTime();
        }

        // 2. 실패 오브젝트를 내 머리 위로 TP 시키고 활성화
        if (failObject != null)
        {
            failObject.transform.position = transform.position + objectOffset;
            failObject.SetActive(true);
        }

        // 3. 실패 UI용 텍스트에 0점과 등반 기록 출력
        if (failScoreText != null)
        {
            int minutes = Mathf.FloorToInt(clearTime / 60f);
            int seconds = Mathf.FloorToInt(clearTime % 60f);

            failScoreText.text = $"💀 등반 실패...\n" +
                                 $"⏱️ 버틴 시간: {minutes:00}:{seconds:00}\n" +
                                 $"--------------------\n" +
                                 $"TOTAL SCORE: 0점";
        }
        
        // PlayerHP에서 이미 Time.timeScale = 0f을 해주므로 여기서는 텍스트만 처리합니다.
    }

    void ExitGame()
    {
        Debug.Log("🚪 게임 종료 처리");
        Time.timeScale = 1f; 

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}