using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject mainMenuCanvas;
    public Button playButton;

    [Header("Prologue UI")]
    public GameObject prologueCanvas;
    public TMP_Text prologueText;
    
    public TypewriterEffect typewriterEffect;

    [Header("Settings")]
    public string gameSceneName = "SampleScene"; // GANTI DENGAN NAMA SCENE GAME KAMU NANTI!
    public float textStayDuration = 2f; // Dikurangi sedikit agar gabosan nungguin abis ngetik
    public float fadeDuration = 1.0f;

    [TextArea(2, 5)]
    public string[] prologueLines;

    private void Start()
    {
        // Pastikan Menu tampil, Prologue sembunyi di awal
        mainMenuCanvas.SetActive(true);
        prologueCanvas.SetActive(false);

        // Pasang fungsi ke tombol Play
        playButton.onClick.AddListener(StartPrologue);
    }

    public void StartPrologue()
    {
        // Hilangkan menu utama
        mainMenuCanvas.SetActive(false);
        // Tampilkan layar hitam prologue
        prologueCanvas.SetActive(true);
        // Teks awal jadikan putih penuh (alpha 1) karena akan diketik satu per satu
        SetTextAlpha(1);
        prologueText.text = "";

        StartCoroutine(PrologueRoutine());
    }

    private IEnumerator PrologueRoutine()
    {
        // Jeda bentar sebelum teks pertama muncul (biar dramatis)
        yield return new WaitForSeconds(1f);

        // Putar semua kalimat prologue satu per satu
        foreach (string line in prologueLines)
        {
            // Mulai ngetik
            SetTextAlpha(1); 
            typewriterEffect.StartTyping(prologueText, line);

            // Tunggu sampai typewriter selesai ngetik huruf terakhir
            while (typewriterEffect.IsTyping)
            {
                yield return null;
            }

            // Tunggu orang baca setelah selesai ngetik
            yield return new WaitForSeconds(textStayDuration);

            // Fade Out (Menghilang perlahan)
            if (line != prologueLines[prologueLines.Length - 1]) // Kecuali kalimat terakhir
            {
                yield return FadeText(1f, 0f, fadeDuration);
                prologueText.text = ""; // Bersihkan text biar siap ngetik kalimat baru
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Tahan kalimat terakhir agak lama
        yield return new WaitForSeconds(1f);

        // Tunggu transisi masuk ke gameplay perlahan
        prologueText.text = "Loading...";
        
        // Sembunyikan dan kunci cursor sebelum masuk ke dalam game
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Pindah Scene
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        Color c = prologueText.color;
        c.a = startAlpha;
        prologueText.color = c;

        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            prologueText.color = c;
            yield return null;
        }

        c.a = endAlpha;
        prologueText.color = c;
    }

    private void SetTextAlpha(float alpha)
    {
        Color c = prologueText.color;
        c.a = alpha;
        prologueText.color = c;
    }
}
