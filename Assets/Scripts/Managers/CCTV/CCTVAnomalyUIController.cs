using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CCTVAnomalyUIController : MonoBehaviour
{
    public static CCTVAnomalyUIController Instance;

    [Header("Warning & Header UI")]
    [SerializeField] private GameObject warningBanner;
    [SerializeField] private TMP_Text warningBannerText;

    [Header("Monster 1: QTE Minigame")]
    [SerializeField] private Button emergencyLockdownBtn;
    [SerializeField] private TMP_Text lockdownBtnText;
    [SerializeField] private GameObject qteContainer;
    [SerializeField] private RectTransform qteBarBase;
    [SerializeField] private RectTransform qteGreenZone;
    [SerializeField] private RectTransform qtePointer;
    [SerializeField] private TMP_Text qteStatusText;
    [SerializeField] private float qtePointerSpeed = 2.8f;

    [Header("Monster 2: Glitch Stare & Hold Lockdown")]
    [SerializeField] private GameObject glitchStaticOverlay;
    [SerializeField] private GameObject focusContainer;
    [SerializeField] private Image focusFillBar;
    [SerializeField] private TMP_Text focusStatusText;
    [SerializeField] private GameObject holdLockdownContainer;
    [SerializeField] private Image holdLockdownFillBar;
    [SerializeField] private TMP_Text holdLockdownStatusText;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip qteSuccessSfx;
    [SerializeField] private AudioClip qteFailSfx;
    [SerializeField] private AudioClip gateCloseSfx;

    // Monster 1 QTE State
    private bool isMonster1Active = false;
    private bool isQTEActive = false;
    private bool isQTEPassed = false;
    private bool hasTriggeredCrawl = false;
    private float qteTimer = 0f;
    private float barHalfWidth = 140f;
    private float greenZoneHalfWidth = 38f;

    // Monster 2 Stare 3s State
    private bool isStarePhaseActive = false;
    private float currentStareProgress = 0f;
    private float requiredStareTime = 3.0f;
    private int targetAnomalyCameraIndex = -1;

    // Monster 2 Hold Lockdown State
    private bool isHoldLockdownActive = false;
    private float currentHoldTime = 0f;
    private float requiredHoldTime = 1.5f;

    private void Awake()
    {
        Instance = this;

        if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
        if (uiAudioSource == null) uiAudioSource = gameObject.AddComponent<AudioSource>();

        HideAllAnomalyUI();

        if (emergencyLockdownBtn != null)
        {
            emergencyLockdownBtn.onClick.AddListener(StartQTE);
        }
    }

    private void Update()
    {
        int currentCam = CCTVManager.Instance != null ? CCTVManager.Instance.CurrentIndex : -1;
        bool isLookingAtTargetCam = (currentCam == targetAnomalyCameraIndex);

        // 1. Monster 1 Logic: Mulai merangkak & tampilkan QTE bar HANYA jika player sudah berada di kamera monster!
        if (isMonster1Active)
        {
            if (isLookingAtTargetCam)
            {
                if (!hasTriggeredCrawl)
                {
                    hasTriggeredCrawl = true;
                    if (CCTVAnomalyManager.Instance != null)
                    {
                        CCTVAnomalyManager.Instance.OnPlayerViewedMonster1Camera();
                    }
                }

                if (!isQTEActive && !isQTEPassed)
                {
                    StartQTE();
                }

                if (qteContainer != null && !qteContainer.activeSelf && !isQTEPassed)
                {
                    qteContainer.SetActive(true);
                }

                if (warningBannerText != null)
                    warningBannerText.text = $"⚠️ <b>PERINGATAN: ANOMALI DI CAM 0{targetAnomalyCameraIndex + 1}! TEKAN [SPASI] SAAT JARUM DI ZONA HIJAU!</b>";
            }
            else
            {
                // Sembunyikan QTE bar jika player sedang melihat kamera lain agar tidak menghalangi layar
                if (qteContainer != null && qteContainer.activeSelf)
                {
                    qteContainer.SetActive(false);
                }

                if (warningBannerText != null)
                    warningBannerText.text = $"⚠️ <b>PERINGATAN: ANOMALI TERDETEKSI DI CAM 0{targetAnomalyCameraIndex + 1}! CARI KAMERA TERSEBUT!</b>";
            }
        }

        // QTE Minigame Logic
        if (isQTEActive && !isQTEPassed && isLookingAtTargetCam)
        {
            qteTimer += Time.deltaTime * qtePointerSpeed;
            float pingPongX = Mathf.Sin(qteTimer * Mathf.PI) * barHalfWidth;
            if (qtePointer != null)
            {
                qtePointer.anchoredPosition = new Vector2(pingPongX, qtePointer.anchoredPosition.y);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                CheckQTEHit(pingPongX);
            }
        }

        // 2. Monster 2 Phase 1: Glitch Stare (3 Detik)
        if (isStarePhaseActive)
        {
            if (glitchStaticOverlay != null)
            {
                glitchStaticOverlay.SetActive(isLookingAtTargetCam);
                if (isLookingAtTargetCam)
                {
                    Image gImg = glitchStaticOverlay.GetComponent<Image>();
                    if (gImg != null) gImg.color = new Color(1f, 0.1f, 0.1f, Random.Range(0.2f, 0.45f));
                }
            }

            if (isLookingAtTargetCam)
            {
                if (focusContainer != null && !focusContainer.activeSelf) focusContainer.SetActive(true);

                currentStareProgress += Time.deltaTime;
                float fill = Mathf.Clamp01(currentStareProgress / requiredStareTime);

                if (focusFillBar != null) focusFillBar.fillAmount = fill;
                if (focusStatusText != null) focusStatusText.text = $"MENATAP DISTORSI SINYAL: {(int)(fill * 100)}%";

                if (currentStareProgress >= requiredStareTime)
                {
                    isStarePhaseActive = false;
                    if (glitchStaticOverlay != null) glitchStaticOverlay.SetActive(false);
                    if (focusContainer != null) focusContainer.SetActive(false);

                    StartCoroutine(JumpscareScreenShakeRoutine());

                    if (CCTVAnomalyManager.Instance != null)
                    {
                        CCTVAnomalyManager.Instance.OnStare3SecondsCompleted();
                    }
                }
            }
            else
            {
                currentStareProgress = Mathf.Max(0f, currentStareProgress - Time.deltaTime * 2f);
                if (focusFillBar != null) focusFillBar.fillAmount = currentStareProgress / requiredStareTime;
                if (focusStatusText != null) focusStatusText.text = "<color=#FFCC00>CARI KAMERA YANG MENGALAMI GLITCH!</color>";
            }
        }

        // 3. Monster 2 Phase 2: Hold Lockdown (Tahan Tombol / Spasi)
        if (isHoldLockdownActive)
        {
            bool isHolding = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

            if (isHolding)
            {
                currentHoldTime += Time.deltaTime;
                float fill = Mathf.Clamp01(currentHoldTime / requiredHoldTime);

                if (holdLockdownFillBar != null) holdLockdownFillBar.fillAmount = fill;
                if (holdLockdownStatusText != null) holdLockdownStatusText.text = $"TAHAN UNTUK LOCKDOWN... {(int)(fill * 100)}%";

                if (currentHoldTime >= requiredHoldTime)
                {
                    isHoldLockdownActive = false;
                    if (holdLockdownStatusText != null) holdLockdownStatusText.text = "<color=#00FF66>LOCKDOWN GERBANG BERHASIL!</color>";

                    PlaySound(qteSuccessSfx);
                    PlaySound(gateCloseSfx);

                    StartCoroutine(HoldLockdownPassedRoutine());
                }
            }
            else
            {
                currentHoldTime = Mathf.Max(0f, currentHoldTime - Time.deltaTime * 3f);
                float fill = Mathf.Clamp01(currentHoldTime / requiredHoldTime);
                if (holdLockdownFillBar != null) holdLockdownFillBar.fillAmount = fill;
                if (holdLockdownStatusText != null) holdLockdownStatusText.text = "TAHAN <b>[SPASI / KLIK MOUSE]</b> UNTUK LOCKDOWN!";
            }
        }
    }

    public void ShowMonster1UI(int cameraIndex)
    {
        HideAllAnomalyUI();
        targetAnomalyCameraIndex = cameraIndex;
        isMonster1Active = true;
        hasTriggeredCrawl = false;

        if (warningBanner != null)
        {
            warningBanner.SetActive(true);
            if (warningBannerText != null)
                warningBannerText.text = $"⚠️ <b>PERINGATAN: ANOMALI DI CAM 0{cameraIndex + 1}! CARI KAMERA TERSEBUT!</b>";
        }
    }

    public void ShowMonster2GlitchPhase(int cameraIndex)
    {
        HideAllAnomalyUI();
        targetAnomalyCameraIndex = cameraIndex;
        isStarePhaseActive = true;
        currentStareProgress = 0f;

        if (warningBanner != null)
        {
            warningBanner.SetActive(true);
            if (warningBannerText != null)
                warningBannerText.text = "⚠️ <b>DISTORSI SINYAL MISTERIUS — TEMUKAN KAMERA & TATAP SELAMA 3 DETIK!</b>";
        }

        if (focusContainer != null)
        {
            focusContainer.SetActive(true);
            if (focusFillBar != null) focusFillBar.fillAmount = 0f;
            if (focusStatusText != null) focusStatusText.text = "CARI KAMERA YANG MENGALAMI GLITCH...";
        }
    }

    public void ShowMonster2HoldLockdownPhase()
    {
        isStarePhaseActive = false;
        if (focusContainer != null) focusContainer.SetActive(false);

        isHoldLockdownActive = true;
        currentHoldTime = 0f;

        if (warningBanner != null)
        {
            warningBanner.SetActive(true);
            if (warningBannerText != null)
                warningBannerText.text = "🚨 <b>ENTITAS MENYERANG KAMERA! TAHAN TOMBOL UNTUK LOCKDOWN!</b>";
        }

        if (holdLockdownContainer != null)
        {
            holdLockdownContainer.SetActive(true);
            if (holdLockdownFillBar != null) holdLockdownFillBar.fillAmount = 0f;
            if (holdLockdownStatusText != null) holdLockdownStatusText.text = "TAHAN <b>[SPASI / KLIK MOUSE]</b> UNTUK LOCKDOWN!";
        }
    }

    public void StartQTE()
    {
        if (emergencyLockdownBtn != null) emergencyLockdownBtn.gameObject.SetActive(false);

        if (qteContainer != null) qteContainer.SetActive(true);
        isQTEActive = true;
        isQTEPassed = false;
        qteTimer = 0f;

        if (qteStatusText != null)
        {
            qteStatusText.text = "TEKAN <b>[SPASI]</b> SAAT JARUM DI ZONA HIJAU!";
            qteStatusText.color = Color.white;
        }
    }

    private void CheckQTEHit(float pointerX)
    {
        if (Mathf.Abs(pointerX) <= greenZoneHalfWidth)
        {
            isQTEPassed = true;
            isQTEActive = false;
            isMonster1Active = false;

            if (qteStatusText != null)
            {
                qteStatusText.text = "<color=#00FF66><b>HIT! SUCCESS</b>\nGERBANG DARURAT TERKUNCI!</color>";
            }

            PlaySound(qteSuccessSfx);
            PlaySound(gateCloseSfx);

            StartCoroutine(QTEPassedRoutine());
        }
        else
        {
            if (qteStatusText != null)
            {
                qteStatusText.text = "<color=#FF3333><b>MISS! COBA LAGI!</b></color>";
            }
            PlaySound(qteFailSfx);
            StartCoroutine(ShakeQTEBar());
        }
    }

    private IEnumerator QTEPassedRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        HideAllAnomalyUI();

        if (CCTVAnomalyManager.Instance != null)
        {
            CCTVAnomalyManager.Instance.OnQTEGateSuccess();
        }
    }

    private IEnumerator HoldLockdownPassedRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        HideAllAnomalyUI();

        if (CCTVAnomalyManager.Instance != null)
        {
            CCTVAnomalyManager.Instance.OnMonster2LockdownSuccess();
        }
    }

    private IEnumerator ShakeQTEBar()
    {
        if (qteBarBase == null) yield break;

        Vector2 origPos = qteBarBase.anchoredPosition;
        for (int i = 0; i < 6; i++)
        {
            qteBarBase.anchoredPosition = origPos + new Vector2(Random.Range(-8f, 8f), 0);
            yield return new WaitForSeconds(0.04f);
        }
        qteBarBase.anchoredPosition = origPos;
    }

    private IEnumerator JumpscareScreenShakeRoutine()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) yield break;

        Vector2 origPos = rt.anchoredPosition;
        for (int i = 0; i < 10; i++)
        {
            rt.anchoredPosition = origPos + new Vector2(Random.Range(-15f, 15f), Random.Range(-10f, 10f));
            yield return new WaitForSeconds(0.03f);
        }
        rt.anchoredPosition = origPos;
    }

    public void HideAllAnomalyUI()
    {
        isMonster1Active = false;
        isQTEActive = false;
        isQTEPassed = false;
        isStarePhaseActive = false;
        isHoldLockdownActive = false;
        hasTriggeredCrawl = false;

        if (warningBanner != null) warningBanner.SetActive(false);
        if (emergencyLockdownBtn != null) emergencyLockdownBtn.gameObject.SetActive(false);
        if (qteContainer != null) qteContainer.SetActive(false);
        if (focusContainer != null) focusContainer.SetActive(false);
        if (holdLockdownContainer != null) holdLockdownContainer.SetActive(false);
        if (glitchStaticOverlay != null) glitchStaticOverlay.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(clip, 0.9f);
        }
    }
}
