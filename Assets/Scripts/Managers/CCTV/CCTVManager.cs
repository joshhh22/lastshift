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
    private float blinkTimer;
    private bool blinkState = true;
    private bool[] visited;
    private bool objectiveCompleted;
    private float switchCooldown = 0.15f;
    private float lastSwitchTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        visited = new bool[cameras.Length];
        CloseCCTV();
    }

    public void OpenCCTV()
    {
        UIManager.Instance.ShowComputer();

        cctvUI.SetActive(true);

        currentIndex = 0;

        objectiveCompleted = false;

        visited = new bool[cameras.Length];
        visited[0] = true;

        recLabel.gameObject.SetActive(true);
        UpdateCamera();
    }

    public void CloseCCTV()
    {
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }

        recLabel.gameObject.SetActive(false);
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
        if (!recLabel.gameObject.activeSelf)
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
            cam.gameObject.SetActive(false);
        }

        if (cameras.Length > 0)
        {
            cameras[currentIndex].gameObject.SetActive(true);

            cameraLabel.text = $"CAM {currentIndex + 1:00}";
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
