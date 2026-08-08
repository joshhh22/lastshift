using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Intensity")]
    [SerializeField] private float normalIntensity = 6f;
    [SerializeField] private float dimIntensity = 2f;

    [Header("Timing")]
    [SerializeField] private Vector2 waitBetweenFlickers = new Vector2(8f, 20f);

    [SerializeField] private int minFlashes = 2;
    [SerializeField] private int maxFlashes = 5;

    [SerializeField] private Vector2 flashDuration = new Vector2(0.03f, 0.08f);

    private Light lightSource;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        lightSource.intensity = normalIntensity;
    }

    private void Start()
    {
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(
                    waitBetweenFlickers.x,
                    waitBetweenFlickers.y));

            int flashes =
                Random.Range(minFlashes, maxFlashes + 1);

            for (int i = 0; i < flashes; i++)
            {
                lightSource.intensity = dimIntensity;

                yield return new WaitForSeconds(
                    Random.Range(
                        flashDuration.x,
                        flashDuration.y));

                lightSource.intensity = normalIntensity;

                yield return new WaitForSeconds(
                    Random.Range(
                        flashDuration.x,
                        flashDuration.y));
            }
        }
    }
}