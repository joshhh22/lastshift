using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SevereLightFlicker : MonoBehaviour
{
    private Light myLight;
    private float originalIntensity;
    
    private void Awake()
    {
        myLight = GetComponent<Light>();
        originalIntensity = myLight.intensity;
    }

    private void Start()
    {
        // Hanya aktif menjadi sangat seram jika Day 4 ke atas
        if (DayManager.Instance != null && DayManager.Instance.CurrentDay >= GameDay.Day4)
        {
            StartCoroutine(HorrorLightRoutine());
        }
    }

    private IEnumerator HorrorLightRoutine()
    {
        while (true)
        {
            // Lampu menyala normal selama beberapa detik
            myLight.intensity = originalIntensity;
            yield return new WaitForSeconds(Random.Range(5f, 15f));

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
                    // AudioManager.Instance.PlayPowerDown(); // Bisa diaktifkan kalau punya sound effectnya
                    yield return new WaitForSeconds(Random.Range(3f, 7f)); 
                    break;
            }
        }
    }
}
