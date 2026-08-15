using TMPro;
using UnityEngine;

public class CCTVManager : MonoBehaviour
{
    public static CCTVManager Instance;

    [Header("CCTV Cameras")]
    [SerializeField] private Camera[] cameras;

    [Header("UI")]
    [SerializeField] private TMP_Text cameraLabel;
    [SerializeField] private TMP_Text recLabel;
    [SerializeField] private GameObject cctvUI;

    private int currentIndex;
    public int CurrentIndex => currentIndex;
    private float blinkTimer;
    private bool blinkState = true;
    private bool[] visited;
    private bool objectiveCompleted;
    private float switchCooldown = 0.15f;
    private float lastSwitchTime;

    private RenderTexture cctvRenderTexture;
    public RenderTexture CctvRenderTexture => cctvRenderTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (cctvRenderTexture == null)
        {
            cctvRenderTexture = new RenderTexture(1280, 720, 24);
            cctvRenderTexture.name = "CCTV_LiveFeed_RT";
        }
    }

    private void Start()
    {
        visited = new bool[cameras.Length];

        if (cctvRenderTexture == null)
        {
            cctvRenderTexture = new RenderTexture(1280, 720, 24);
            cctvRenderTexture.name = "CCTV_LiveFeed_RT";
        }

        foreach (Camera cam in cameras)
        {
            if (cam != null)
            {
                cam.targetTexture = cctvRenderTexture;
            }
        }

        CloseCCTV();
    }

    public void OpenCCTV()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowComputer();

        if (FrutigerAeroComputerUI.Instance == null && cctvUI != null)
        {
            cctvUI.SetActive(true);
        }

        if (FrutigerAeroComputerUI.Instance != null && FrutigerAeroComputerUI.Instance.cctvViewportRawImage != null)
        {
            FrutigerAeroComputerUI.Instance.cctvViewportRawImage.texture = cctvRenderTexture;
        }

        currentIndex = 0;
        objectiveCompleted = false;

        visited = new bool[cameras.Length];
        if (visited.Length > 0) visited[0] = true;

        if (recLabel != null) recLabel.gameObject.SetActive(true);
        UpdateCamera();
    }

    public void CloseCCTV()
    {
        if (cameras != null)
        {
            foreach (Camera cam in cameras)
            {
                if (cam != null) cam.gameObject.SetActive(false);
            }
        }

        if (recLabel != null)
            recLabel.gameObject.SetActive(false);

        if (cctvUI != null && FrutigerAeroComputerUI.Instance == null)
            cctvUI.SetActive(false);
    }

    public void NextCamera()
    {
        if (Time.time - lastSwitchTime < switchCooldown)
            return;

        lastSwitchTime = Time.time;

        if (cameras.Length == 0)
            return;

        currentIndex++;

        if (currentIndex >= cameras.Length)
            currentIndex = 0;

        UpdateCamera();
        visited[currentIndex] = true;
        CheckObjective();
    }

    public void PreviousCamera()
    {
        if (cameras.Length == 0)
            return;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = cameras.Length - 1;

        UpdateCamera();
        visited[currentIndex] = true;
        CheckObjective();
    }

    private void Update()
    {
        if (recLabel == null || !recLabel.gameObject.activeInHierarchy)
            return;

        blinkTimer += Time.deltaTime;

        if (blinkTimer >= 0.5f)
        {
            blinkTimer = 0f;
            blinkState = !blinkState;
            recLabel.enabled = blinkState;
        }
    }

    private void UpdateCamera()
    {
        foreach (Camera cam in cameras)
        {
            if (cam != null)
            {
                cam.targetTexture = cctvRenderTexture;
                cam.gameObject.SetActive(false);
            }
        }

        if (cameras.Length > 0 && currentIndex < cameras.Length)
        {
            if (cameras[currentIndex] != null)
            {
                cameras[currentIndex].gameObject.SetActive(true);
            }

            string[] camNames = new string[] { "CAM 01: LOBBY COUNTER", "CAM 02: STAIRS & PLATFORM", "CAM 03: BOOTH PERIMETER" };
            string nameStr = currentIndex < camNames.Length ? camNames[currentIndex] : $"CAM {currentIndex + 1:00}";

            if (cameraLabel != null)
                cameraLabel.text = $"📍 <b>{nameStr}</b>";

            if (FrutigerAeroComputerUI.Instance != null && FrutigerAeroComputerUI.Instance.cctvCameraLabel != null)
            {
                FrutigerAeroComputerUI.Instance.cctvCameraLabel.text = $"📍 <b>{nameStr}</b>";
            }
        }
    }

    private void CheckObjective()
    {
        if (objectiveCompleted)
            return;

        if (ObjectiveManager.Instance.GetCurrentObjective() != "Check CCTV")
            return;

        foreach (bool cam in visited)
        {
            if (!cam)
                return;
        }

        objectiveCompleted = true;

        ObjectiveManager.Instance.CompleteObjective();
    }
}
