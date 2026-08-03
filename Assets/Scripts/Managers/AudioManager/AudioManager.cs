using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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
    // CORE
    //==============================

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}