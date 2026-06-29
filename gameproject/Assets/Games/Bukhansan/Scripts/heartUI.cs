using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    [Header("하트 UI 설정")]
    public Image[] hearts;        // 하트 이미지 배열
    public GameObject heartsPanel; // (선택사항) 게임오버 시 하트 UI 전체를 숨기고 싶다면 부모 오브젝트 연결

    [Header("체력 설정")]
    public int hp = 3;
    public bool isInvincible = false;

    void Start()
    {
        UpdateHearts();
    }

    // 외부(돌멩이 등)에서 데미지를 줄 때 호출하는 함수
    public void TakeDamage(int damage = 1)
    {
        if (isInvincible || hp <= 0) return;

        hp -= damage;

        if (hp < 0)
            hp = 0;

        UpdateHearts();

        if (hp <= 0)
        {
            GameOver();
            return;
        }

        StartCoroutine(InvincibilityCoroutine());
    }

    // 무적 상태 코루틴
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.5f);
        isInvincible = false;
    }

    // 하트 개수 갱신 UI
    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].enabled = i < hp;
            }
        }
    }

    // 체력이 0이 되었을 때 호출
    void GameOver()
    {
        Debug.Log("💀 게임 오버 - 플레이어 사망");

        // 1. 게임 내 시간(물리 연산, 오브젝트 움직임 등) 정지
        Time.timeScale = 0f; 

        // 2. (선택사항) 하트 전체 판넬 숨기기
        if (heartsPanel != null)
        {
            heartsPanel.SetActive(false);
        }
    }
}