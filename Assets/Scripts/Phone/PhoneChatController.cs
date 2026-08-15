using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ChatMessageItem
{
    public bool isPlayer; // false = Incoming (Left Gray), true = Outgoing Player (Right Blue)
    [TextArea(2, 4)]
    public string text;
    public float readDelay; // Delay before typing / sending
}

public class PhoneChatController : MonoBehaviour
{
    public static PhoneChatController Instance;

    [Header("Header UI")]
    [SerializeField] private TMP_Text contactTitleText;

    [Header("Chat Scroll View")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform chatContainer;

    [Header("Bubble Templates")]
    [SerializeField] private GameObject incomingBubbleTemplate;
    [SerializeField] private GameObject outgoingBubbleTemplate;
    [SerializeField] private GameObject typingIndicatorTemplate;

    [Header("Input Bar & Hints")]
    [SerializeField] private Button inputBarButton;
    [SerializeField] private TMP_Text inputPlaceholderText;
    [SerializeField] private TMP_Text bottomHintText;

    [Header("Audio")]
    [SerializeField] private AudioClip messageSendSfx;
    [SerializeField] private AudioClip messageReceiveSfx;

    private Coroutine chatRoutine;
    private int currentPlayedDay = -1;
    private bool isConversationFinished = false;
    private bool isWaitingForPlayerReply = false;
    private bool playerTriggeredReply = false;

    private void Awake()
    {
        Instance = this;

        if (incomingBubbleTemplate != null) incomingBubbleTemplate.SetActive(false);
        if (outgoingBubbleTemplate != null) outgoingBubbleTemplate.SetActive(false);
        if (typingIndicatorTemplate != null) typingIndicatorTemplate.SetActive(false);

        if (inputBarButton != null)
        {
            inputBarButton.onClick.AddListener(OnPlayerClickedReply);
        }
    }

    private void OnEnable()
    {
        StartCurrentDayConversation();
    }

    private void OnDisable()
    {
        if (chatRoutine != null)
        {
            StopCoroutine(chatRoutine);
            chatRoutine = null;
        }
        isWaitingForPlayerReply = false;
        playerTriggeredReply = false;
    }

    private void Update()
    {
        if (isWaitingForPlayerReply)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnPlayerClickedReply();
            }
        }
    }

    public void OnPlayerClickedReply()
    {
        if (isWaitingForPlayerReply)
        {
            playerTriggeredReply = true;
            isWaitingForPlayerReply = false;
        }
    }

    public void StartCurrentDayConversation()
    {
        if (DayManager.Instance == null) return;

        int dayInt = (int)DayManager.Instance.CurrentDay;
        
        // Bersihkan chat lama jika hari berganti
        if (currentPlayedDay != dayInt)
        {
            ClearChatContainer();
            currentPlayedDay = dayInt;
            isConversationFinished = false;
        }

        if (chatRoutine != null)
        {
            StopCoroutine(chatRoutine);
            chatRoutine = null;
        }

        if (!isConversationFinished)
        {
            chatRoutine = StartCoroutine(PlayConversationRoutine(DayManager.Instance.CurrentDay));
        }
    }

    private void ClearChatContainer()
    {
        if (chatContainer == null) return;

        foreach (Transform child in chatContainer)
        {
            if (child.gameObject == incomingBubbleTemplate || 
                child.gameObject == outgoingBubbleTemplate || 
                child.gameObject == typingIndicatorTemplate)
                continue;

            Destroy(child.gameObject);
        }
    }

    private IEnumerator PlayConversationRoutine(GameDay day)
    {
        string contactName = "Messages";
        List<ChatMessageItem> messages = GetConversationForDay(day, out contactName);

        if (contactTitleText != null)
        {
            contactTitleText.text = $"Messages • {contactName}";
        }

        for (int i = 0; i < messages.Count; i++)
        {
            var msg = messages[i];

            if (!msg.isPlayer)
            {
                // Pesan dari lawan bicara (Incoming)
                if (i > 0)
                {
                    // Tampilkan animasi mengetik "..."
                    if (typingIndicatorTemplate != null)
                    {
                        typingIndicatorTemplate.SetActive(true);
                        typingIndicatorTemplate.transform.SetAsLastSibling();
                        ScrollToBottom();
                        yield return new WaitForSeconds(1.2f);
                        typingIndicatorTemplate.SetActive(false);
                    }
                }

                SpawnBubble(incomingBubbleTemplate, msg.text, false);
                PlaySound(messageReceiveSfx);
                ScrollToBottom();
            }
            else
            {
                // Menunggu pemain menekan tombol / klik layar untuk membalas
                isWaitingForPlayerReply = true;
                playerTriggeredReply = false;

                if (inputPlaceholderText != null)
                {
                    inputPlaceholderText.text = "<color=#007AFF><b>[KLIK / ENTER UNTUK MEMBALAS]</b></color>";
                }
                if (bottomHintText != null)
                {
                    bottomHintText.text = "[!] KLIK LAYAR / ENTER UNTUK MEMBALAS  |  [TAB] TUTUP";
                }

                while (!playerTriggeredReply)
                {
                    yield return null;
                }

                if (inputPlaceholderText != null)
                {
                    inputPlaceholderText.text = "Message...";
                }

                SpawnBubble(outgoingBubbleTemplate, msg.text, true);
                PlaySound(messageSendSfx);
                ScrollToBottom();
            }

            yield return new WaitForSeconds(0.4f);
        }

        isConversationFinished = true;
        isWaitingForPlayerReply = false;

        if (bottomHintText != null)
        {
            bottomHintText.text = "[TAB] TUTUP PESAN";
        }

        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.GetCurrentObjective() == "Check Phone")
        {
            ObjectiveManager.Instance.CompleteObjective();
        }
    }

    private void SpawnBubble(GameObject template, string text, bool isPlayer)
    {
        if (template == null || chatContainer == null) return;

        GameObject bubble = Instantiate(template, chatContainer);
        bubble.SetActive(true);
        bubble.transform.SetAsLastSibling();

        TMP_Text txt = bubble.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.text = text;
        }

        StartCoroutine(AnimateBubblePop(bubble.GetComponent<RectTransform>()));
    }

    private IEnumerator AnimateBubblePop(RectTransform rt)
    {
        if (rt == null) yield break;

        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.85f, 0.85f, 1f);
        Vector3 targetScale = Vector3.one;

        rt.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            rt.localScale = Vector3.LerpUnclamped(startScale, targetScale, 1f + 0.15f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        rt.localScale = targetScale;
    }

    private void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomRoutine());
    }

    private IEnumerator ScrollToBottomRoutine()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.8f);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPhoneNotification();
        }
    }

    private List<ChatMessageItem> GetConversationForDay(GameDay day, out string contactName)
    {
        List<ChatMessageItem> list = new List<ChatMessageItem>();

        switch (day)
        {
            case GameDay.Day1:
                contactName = "Ibu";
                list.Add(new ChatMessageItem { isPlayer = false, text = "Nak, kamu sudah sampai di stasiun?", readDelay = 1.6f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Sudah bu, aku baru saja mulai shift malam ini.", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "Cuaca malam ini katanya makin beku, pakai jaket tebalmu ya. Ibu dan adikmu menunggu jatah ransum darimu besok pagi.", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Iya bu, jangan lupa rapatkan jendela rumah ya. Kabarnya badai salju malam ini memburuk.", readDelay = 2.0f });
                break;

            case GameDay.Day2:
                contactName = "Info Pusat";
                list.Add(new ChatMessageItem { isPlayer = false, text = "[PENGUMUMAN PENTING]\nPipa termal penghangat di Stasiun Frostgate meledak. Sektor dikarantina total!", readDelay = 1.6f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Frostgate meledak?! Berarti tidak ada kereta yang boleh ke sana?", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "Dilarang keras meloloskan penumpang menuju Frostgate! Tolak semua tiketnya!", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Dimengerti. Tidak akan ada penumpang gelap yang lolos ke Frostgate di awasanku.", readDelay = 1.8f });
                break;

            case GameDay.Day3:
                contactName = "Info Pusat";
                list.Add(new ChatMessageItem { isPlayer = false, text = "[DARURAT MEDIS]\nWabah mematikan 'Paru-paru Es' dikonfirmasi merebak di Stasiun Snowtrench.", readDelay = 1.6f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Bagaimana protokol penyaringan untuk penumpang dari Snowtrench?", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "TOLAK semua tiket yang BERASAL (Origin) dari Snowtrench demi mencegah penularan di dalam gerbong!", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Siap. Asal stasiun = Snowtrench akan langsung masuk daftar hitam (Blacklist).", readDelay = 1.8f });
                break;

            case GameDay.Day4:
                contactName = "Supervisor";
                list.Add(new ChatMessageItem { isPlayer = false, text = "[PROTOKOL EVAKUASI KRITIS]\nGenerator utama The Core melemah! Kuota pengungsi dipangkas!", readDelay = 1.6f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Apa instruksi khusus untuk pemeriksaan tiket malam ini, pak?", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "Mulai hari ini, HANYA WANITA yang diizinkan naik ke kereta. TOLAK SEMUA PRIA (Male) tanpa terkecuali!", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Ini gila... Pria dengan tiket Valid juga harus dibuang ke suhu minus derajat?", readDelay = 2.0f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "Ini perintah langsung dari Direksi. Laksanakan atau posisimu digantikan!", readDelay = 1.8f });
                break;

            case GameDay.Day5:
                contactName = "Supervisor";
                list.Add(new ChatMessageItem { isPlayer = false, text = "PERINGATAN: Kereta kita kelebihan beban dan generator hampir korsleting.", readDelay = 1.6f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Banyak penumpang yang memaksa masuk dengan tiket palsu di loket.", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "Pastikan tidak ada tiket palsu yang lolos. Tolak mereka atau kita semua mati kedinginan di dalam kereta!", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Baik, pak. Saya akan periksa fisik kartu lebih teliti lagi.", readDelay = 1.8f });
                break;

            case GameDay.Day6:
                contactName = "Nomor Tidak Dikenal";
                list.Add(new ChatMessageItem { isPlayer = false, text = "Kami tahu lokasimu di loket stasiun...", readDelay = 1.6f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Siapa ini?! Dari mana kamu dapat frekuensi nomor loket?", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "Malam ini ratusan orang beringas dari permukaan akan mencoba menerobos stasiunmu demi kehangatan. Lindungi dirimu sendiri.", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Gerbang baja stasiun terkunci rapat. Kalian tidak akan bisa menembus mesin tiket hidup-hidup!", readDelay = 2.0f });
                break;

            case GameDay.Day7:
                contactName = "Ibu";
                list.Add(new ChatMessageItem { isPlayer = false, text = "(Pesan Gagal Terkirim) ... Sinyal Terputus.", readDelay = 1.5f });
                list.Add(new ChatMessageItem { isPlayer = false, text = "Suhu Darurat: -50°C Terdeteksi di Sektor Permukiman.", readDelay = 1.8f });
                list.Add(new ChatMessageItem { isPlayer = true, text = "Ibu?! Tolong jawab pesanku! Kumohon katakan kalian masih hidup di sana... Ya Tuhan.", readDelay = 2.2f });
                break;

            default:
                contactName = "Tidak Ada Pesan";
                list.Add(new ChatMessageItem { isPlayer = false, text = "Tidak ada pesan baru malam ini.", readDelay = 1.0f });
                break;
        }

        return list;
    }
}
