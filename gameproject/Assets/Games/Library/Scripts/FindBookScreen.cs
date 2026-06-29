using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 책 찾기 2D 화면. 책장 이미지에서 "목표 책"(크롭해서 보여줌)을 찾아 클릭한다.
//
// ★ 책 칸은 buttonsParent(=ShelfImage 위) 밑의 자식 오브젝트(Image+Button)들이다.
//   프리팹(FindScreen.prefab)에서 각 칸을 책 위에 눈으로 배치/크기조절하면,
//   여기서 그 오브젝트가 책장 이미지에서 차지한 영역을 0~1(uv)로 읽어 사용한다.
//   (좌표값을 직접 입력할 필요 없음)
public class FindBookScreen : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RawImage shelfImage;       // 전체 책장 이미지(Texture)
    [SerializeField] private RectTransform buttonsParent; // 책 칸 오브젝트들의 부모(ShelfImage 위에 덮음)
    [SerializeField] private RawImage targetThumb;        // 찾을 책(크롭)
    [SerializeField] private TMP_Text infoText;          // 안내/시간/기회
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    [Header("난이도")]
    [SerializeField] private float timeLimit = 30f;      // 제한시간(초)
    [SerializeField] private float wrongPenalty = 2f;    // 오답 시 시간 깎임(초)
    [SerializeField] private float correctBonus = 0.5f;  // 정답 시 시간 추가(초)

    [Header("찾을 책 썸네일 박스(이 안에서 책 비율 유지)")]
    [SerializeField] private float thumbBoxWidth = 170f;
    [SerializeField] private float thumbBoxHeight = 240f;
    [Tooltip("썸네일 표시 가로 비율. 1=실제 비율, 작을수록 가로를 좁게 표시(늘어짐 보정).")]
    [SerializeField, Range(0.4f, 1f)] private float thumbWidthFactor = 0.8f;

    public bool IsOpen { get; private set; }

    private LibraryInteract owner;
    private readonly List<Rect> cellUVs = new List<Rect>();
    private int targetIndex;
    private int found;     // 찾은 책 수(점수)
    private float timeLeft;
    private bool playing;

    // ⚠️ Awake에서 panel.SetActive(false)를 하면 안 됨!
    // panel == 이 컴포넌트가 붙은 FindScreen 자기 자신이라,
    // Open()이 SetActive(true)로 켜는 순간(=첫 활성화) Awake가 즉시 돌면서
    // 다시 꺼버려 화면이 안 뜬다. (빌더가 이미 꺼진 상태로 씬을 저장하므로 초기 숨김은 그쪽에서 보장)

    public void Open(LibraryInteract from)
    {
        owner = from;
        IsOpen = true;
        if (panel) panel.SetActive(true);
        if (resultPanel) resultPanel.SetActive(false);

        found = 0; timeLeft = timeLimit; playing = true;
        CollectBooks();
        NextTarget();
        UpdateInfo();
    }

    public void Close()
    {
        IsOpen = false;
        playing = false;
        if (panel) panel.SetActive(false);
        if (owner) owner.OnScreenClosed();
    }

    // 결과 화면 "다시 플레이" 버튼 → 점수/시간 초기화하고 같은 화면에서 재시작
    public void Restart()
    {
        if (resultPanel) resultPanel.SetActive(false);
        found = 0; timeLeft = timeLimit; playing = true;
        CollectBooks();
        NextTarget();
        UpdateInfo();
    }

    private void Update()
    {
        if (!IsOpen) return;

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) { Close(); return; }

        if (playing)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f) { timeLeft = 0f; EndGame(); }
            UpdateInfo();
        }
    }

    // buttonsParent 밑에 미리 놓인 책 칸 오브젝트들을 모아, 각 칸이 책장 이미지에서
    // 차지한 영역을 uv(0~1)로 계산하고 클릭 이벤트를 연결한다.
    private void CollectBooks()
    {
        cellUVs.Clear();
        if (buttonsParent == null || shelfImage == null) return;

        Canvas.ForceUpdateCanvases(); // 월드 좌표가 정확하도록 레이아웃 강제 갱신
        var shelfRT = shelfImage.rectTransform;

        int n = buttonsParent.childCount;
        for (int i = 0; i < n; i++)
        {
            var child = buttonsParent.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf) continue;

            var img = child.GetComponent<Image>();
            if (img == null) img = child.gameObject.AddComponent<Image>();
            img.raycastTarget = true; // 투명이어도 클릭은 받게

            var btn = child.GetComponent<Button>();
            if (btn == null) btn = child.gameObject.AddComponent<Button>();

            int idx = cellUVs.Count;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnCellClicked(idx));

            cellUVs.Add(RectToUV(child, shelfRT));
        }
    }

    // 책 칸 오브젝트가 책장 이미지에서 차지하는 영역 → 0~1 uv (RawImage uvRect와 동일 좌표계)
    private static Rect RectToUV(RectTransform book, RectTransform shelf)
    {
        var bc = new Vector3[4];
        var sc = new Vector3[4];
        book.GetWorldCorners(bc);   // 0:좌하 1:좌상 2:우상 3:우하
        shelf.GetWorldCorners(sc);
        float sw = sc[3].x - sc[0].x;
        float sh = sc[1].y - sc[0].y;
        if (Mathf.Abs(sw) < 1e-4f || Mathf.Abs(sh) < 1e-4f) return new Rect(0f, 0f, 1f, 1f);
        float x = (bc[0].x - sc[0].x) / sw;
        float y = (bc[0].y - sc[0].y) / sh;
        float w = (bc[3].x - bc[0].x) / sw;
        float h = (bc[1].y - bc[0].y) / sh;
        return new Rect(x, y, w, h);
    }

    private void NextTarget()
    {
        if (cellUVs.Count == 0) return;
        targetIndex = Random.Range(0, cellUVs.Count);
        if (targetThumb && shelfImage)
        {
            targetThumb.texture = shelfImage.texture;
            targetThumb.uvRect = cellUVs[targetIndex];
            FitThumbAspect();
        }
    }

    // 책 한 칸의 실제 비율(가로/세로)에 맞춰 썸네일을 표시한다(찌그러짐 방지).
    // AspectRatioFitter가 있으면 그것으로 강제(가장 확실), 없으면 sizeDelta로 직접 맞춤.
    private void FitThumbAspect()
    {
        var tex = shelfImage.texture;
        if (tex == null) return;
        Rect uv = targetThumb.uvRect;
        float cellW = uv.width * tex.width;
        float cellH = uv.height * tex.height;
        if (cellW <= 0f || cellH <= 0f) return;
        float aspect = (cellW / cellH) * thumbWidthFactor;   // 표시 가로 비율(작을수록 좁게)

        var fitter = targetThumb.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = aspect;       // 높이 고정, 너비를 비율대로
        }
        else
        {
            float h = thumbBoxHeight;
            float w = h * aspect;
            if (w > thumbBoxWidth) { w = thumbBoxWidth; h = w / aspect; }
            targetThumb.rectTransform.sizeDelta = new Vector2(w, h);
        }
    }

    private void OnCellClicked(int index)
    {
        if (!playing) return;
        if (index == targetIndex)
        {
            found++;                 // 점수 +1, 정답 책을 새로 바꿈
            timeLeft += correctBonus; // 정답 = 시간 +1초
            NextTarget();
            UpdateInfo();
        }
        else
        {
            timeLeft -= wrongPenalty; // 오답 = 시간 -2초
            if (timeLeft <= 0f)       // 패널티로 시간이 다 떨어지면 즉시 종료(음수 방지)
            {
                timeLeft = 0f;
                UpdateInfo();
                EndGame();
                return;
            }
            StopAllCoroutines(); StartCoroutine(Flash());
            UpdateInfo();
        }
    }

    private IEnumerator Flash()
    {
        if (infoText) { var c = infoText.color; infoText.color = Color.red; yield return new WaitForSecondsRealtime(0.2f); infoText.color = c; }
    }

    private void UpdateInfo()
    {
        if (!infoText) return;
        int sec = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)); // 음수 표시 방지
        infoText.text = $"이 책을 찾으세요!   찾은 책 {found}권   ·   ⏱ {sec}초";
    }

    private void EndGame()
    {
        if (!playing) return; // 중복 종료 방지
        playing = false;
        if (resultPanel) resultPanel.SetActive(true);
        if (resultText) resultText.text = $"시간 종료!\n{found}권 찾았어요!";
    }
}
