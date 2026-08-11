using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SevereLightFlicker : MonoBehaviour
{
    private Light myLight;
    private float originalIntensity;

    private float minIdle = 5f;
    private float maxIdle = 15f;

    private void Awake()
    {
        myLight = GetComponent<Light>();
        originalIntensity = myLight.intensity;
    }

    private void Start()
    {
        if (DayManager.Instance == null)
            return;

        GameDay day = DayManager.Instance.CurrentDay;

        // Mulai flicker dari Day 3
        if (day >= GameDay.Day3)
        {
            // Semakin hari, interval idle makin pendek (lebih sering kejadian)
            switch (day)
            {
                case GameDay.Day3: minIdle = 10f; maxIdle = 20f; break;
                case GameDay.Day4: minIdle = 7f;  maxIdle = 15f; break;
                case GameDay.Day5: minIdle = 5f;  maxIdle = 10f; break;
                case GameDay.Day6: minIdle = 3f;  maxIdle = 7f;  break;
                case GameDay.Day7: minIdle = 1f;  maxIdle = 4f;  break;
            }

            StartCoroutine(HorrorLightRoutine());
        }
    }

    private IEnumerator HorrorLightRoutine()
    {
        while (true)
        {
            // Lampu menyala normal selama beberapa detik
            myLight.intensity = originalIntensity;
            yield return new WaitForSeconds(Random.Range(minIdle, maxIdle));

            // Pilih kejadian menakutkan secara acak
            int randomEvent = Random.Range(0, 3);

            switch (randomEvent)
            {
                case 0:
                    // 1. FLICKERING (Kedap-kedip cepat)
                    int flashes = Random.Range(5, 12);
                    for (int i = 0; i < flashes; i++)
                    {
                        myLight.intensity = Random.Range(0f, 0.3f);
                        yield return new WaitForSeconds(Random.Range(0.02f, 0.1f));
                        myLight.intensity = originalIntensity;
                        yield return new WaitForSeconds(Random.Range(0.02f, 0.1f));
                    }
                    break;

                case 1:
                    // 2. DIMMING (Lampu meredup hampir mati selama beberapa saat)
                    myLight.intensity = originalIntensity * 0.15f;
                    yield return new WaitForSeconds(Random.Range(4f, 10f));
                    break;

                case 2:
                    // 3. BLACKOUT (Mati total dalam durasi lama)
                    myLight.intensity = 0f;
                    yield return new WaitForSeconds(Random.Range(3f, 7f));
                    break;
            }
        }
    }
}
