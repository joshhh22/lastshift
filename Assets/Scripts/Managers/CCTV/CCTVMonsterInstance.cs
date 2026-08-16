using System.Collections;
using UnityEngine;

public class CCTVMonsterInstance : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("Monster 1 Roaming")]
    [SerializeField] private float crawlSpeed = 0.75f;
    private bool isCrawling = false;
    private bool isDying = false;

    private Camera attachedCamera;
    private Coroutine activeRoutine;
    private GameObject faceLightObj;

    private void Awake()
    {
        // 1. Matikan dan bersihkan script penumpang/AI jika ada
        var anomPass = GetComponent<AnomalyPassenger>();
        if (anomPass != null) DestroyImmediate(anomPass);

        var npcCtrl = GetComponent<NPCController>();
        if (npcCtrl != null) DestroyImmediate(npcCtrl);

        var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null) DestroyImmediate(navAgent);

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        // Monster 1 Roaming: Hanya merangkak maju jika player sudah melihat kamera
        if (isCrawling && !isDying)
        {
            transform.position += transform.forward * crawlSpeed * Time.deltaTime;
        }
    }

    public void SetupMonster1(Transform spawnPoint, Camera targetCam)
    {
        attachedCamera = targetCam;
        isDying = false;
        isCrawling = false;

        ResetInternalChildOffsets();

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        else if (targetCam != null)
        {
            Vector3 spawnPos = targetCam.transform.position + targetCam.transform.forward * 3.2f + Vector3.down * 0.9f;
            transform.position = spawnPos;
            transform.LookAt(new Vector3(targetCam.transform.position.x, transform.position.y, targetCam.transform.position.z));
        }

        SetRenderersVisible(true);

        if (animator != null)
        {
            animator.applyRootMotion = false;
            int crawlType = Random.Range(0, 2);
            animator.SetInteger("CrawlType", crawlType);
        }
    }

    public void StartCrawling()
    {
        if (isDying || isCrawling) return;

        isCrawling = true;
        if (animator != null)
        {
            animator.SetTrigger("StartCrawl");
        }
    }

    public void SetupMonster2(Transform spawnPoint, Camera targetCam)
    {
        attachedCamera = targetCam;
        isCrawling = false;

        ResetInternalChildOffsets();

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        else if (targetCam != null)
        {
            // Posisikan root transform di bawah lensa agar KEPALA / WAJAH monster pas tepat di tengah layar kamera!
            Vector3 spawnPos = targetCam.transform.position + targetCam.transform.forward * 1.15f + Vector3.down * 1.35f;
            transform.position = spawnPos;
            transform.LookAt(new Vector3(targetCam.transform.position.x, transform.position.y, targetCam.transform.position.z));
        }

        SetRenderersVisible(false);
    }

    public void TriggerMonster1Dying(System.Action onComplete)
    {
        isCrawling = false;
        isDying = true;
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(Monster1DyingRoutine(onComplete));
    }

    private IEnumerator Monster1DyingRoutine(System.Action onComplete)
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        yield return new WaitForSeconds(1.8f);

        onComplete?.Invoke();
        Destroy(gameObject);
    }

    public void TriggerMonster2Scream(AudioClip screamClip, System.Action onComplete)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(Monster2ScreamRoutine(screamClip, onComplete));
    }

    private IEnumerator Monster2ScreamRoutine(AudioClip screamClip, System.Action onComplete)
    {
        // 1. Munculkan monsternya seketika
        SetRenderersVisible(true);

        // 2. Tambahkan pencahayaan merah darah tepat di depan kepala & wajah monster
        if (faceLightObj == null)
        {
            faceLightObj = new GameObject("JumpscareFaceLight");
            faceLightObj.transform.SetParent(transform);
            faceLightObj.transform.position = transform.position + Vector3.up * 1.35f + transform.forward * 0.4f;
            Light lt = faceLightObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.2f, 0.2f);
            lt.range = 4.0f;
            lt.intensity = 8.0f;
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetTrigger("Scream");
        }

        if (screamClip != null && audioSource != null)
        {
            audioSource.clip = screamClip;
            audioSource.volume = 1f;
            audioSource.Play();
        }

        yield return new WaitForSeconds(1.2f);

        onComplete?.Invoke();
    }

    public void DestroyMonster()
    {
        if (faceLightObj != null) Destroy(faceLightObj);
        Destroy(gameObject);
    }

    private void ResetInternalChildOffsets()
    {
        // Menghilangkan offset internal di dalam prefab agar karakter 100% nempel di Gizmo
        foreach (Transform child in transform)
        {
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }
    }

    private void SetRenderersVisible(bool isVisible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = isVisible;
        }
    }
}
