using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Phone")]
    [SerializeField] private AudioClip phoneNotification;

    [Header("UI")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Environment")]
    [SerializeField] private AudioClip doorOpen;
    [SerializeField] private AudioClip gateOpen;
    [SerializeField] private AudioClip trainArrive;

    [Header("Player")]
    [SerializeField] private AudioClip swipeCard;
    [SerializeField] private AudioClip pickupCard;
    [SerializeField] private AudioClip putCard;

    [SerializeField] private AudioClip accessGranted;
    [SerializeField] private AudioClip accessDenied;

    [SerializeField] private AudioClip clockBeep;

    [SerializeField] private AudioClip[] footsteps;

    [Header("Ambient")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip ambientLoop;

    [Header("Station Announcement")]
    [SerializeField] private AudioSource announcementSource;
    [SerializeField] private AudioClip[] announcements;

    [SerializeField] private Vector2 announcementInterval = new Vector2(90f, 180f);

    private Coroutine announcementRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    //==============================
    // BGM
    //==============================

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    //==============================
    // AMBIENT
    //==============================

    public void StartAmbient()
    {
        if (ambientLoop == null)
            return;

        ambientSource.clip = ambientLoop;
        ambientSource.loop = true;
        ambientSource.Play();

        if (announcementRoutine == null)
            announcementRoutine = StartCoroutine(AnnouncementRoutine());
    }

    public void StopAmbient()
    {
        ambientSource.Stop();

        if (announcementRoutine != null)
        {
            StopCoroutine(announcementRoutine);
            announcementRoutine = null;
        }
    }

    //==============================
    // PHONE
    //==============================

    public void PlayPhoneNotification()
    {
        PlaySFX(phoneNotification);
    }

    //==============================
    // UI
    //==============================

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    //==============================
    // PLAYER
    //==============================

    public void PlaySwipeCard()
    {
        PlaySFX(swipeCard);
    }

    public void PlayPickupCard()
    {
        PlaySFX(pickupCard);
    }

    public void PlayPutCard()
    {
        PlaySFX(putCard);
    }

    public void PlayAccessGranted()
    {
        PlaySFX(accessGranted);
    }

    public void PlayAccessDenied()
    {
        PlaySFX(accessDenied);
    }

    public void PlayClockBeep()
    {
        PlaySFX(clockBeep);
    }

    public void PlayFootstep()
    {
        if (footsteps == null || footsteps.Length == 0)
            return;

        AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];

        PlaySFX(clip);
    }

    //==============================
    // ENVIRONMENT
    //==============================

    public void PlayDoorOpen()
    {
        PlaySFX(doorOpen);
    }

    public void PlayGateOpen()
    {
        PlaySFX(gateOpen);
    }

    public void PlayTrainArrive()
    {
        PlaySFX(trainArrive);
    }

    //==============================
    // ANNOUNCEMENT
    //==============================

    private IEnumerator AnnouncementRoutine()
    {
        while (true)
        {
            float wait =
                Random.Range(
                    announcementInterval.x,
                    announcementInterval.y);

            yield return new WaitForSeconds(wait);

            PlayRandomAnnouncement();
        }
    }

    public void PlayRandomAnnouncement()
    {
        if (announcements == null || announcements.Length == 0)
            return;

        AudioClip clip =
            announcements[
                Random.Range(0, announcements.Length)];

        announcementSource.PlayOneShot(clip);
    }

    //==============================
    // CORE
    //==============================

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.pitch = Random.Range(0.96f, 1.04f);

        sfxSource.PlayOneShot(clip);
    }
}