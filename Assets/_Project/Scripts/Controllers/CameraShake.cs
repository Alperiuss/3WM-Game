using UnityEngine;
using System.Collections;
using Unity.Cinemachine; // Unity 6 / Cinemachine 3 Namespace'i

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private CinemachineBasicMultiChannelPerlin perlinComponent;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Aynı obje üzerindeki Perlin (Noise) bileşenini otomatik bulur
        perlinComponent = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Debug.Log(">>> DEPREM BAŞLADI! Süre: " + duration + " sn, Şiddet: " + magnitude);

        if (perlinComponent != null)
        {
            if (AudioManager.Instance != null)
            {
                // AudioManager listesinde ses "Eathquake" adıyla kayıtlı; ikisini de dene
                if (AudioManager.Instance.HasSFX("Earthquake")) AudioManager.Instance.PlaySFX("Earthquake");
                else AudioManager.Instance.PlaySFX("Eathquake");
                
           }
            // Deprem başladığında titremeyi aç ve şiddeti ayarla
            perlinComponent.AmplitudeGain = magnitude * 10f;
            perlinComponent.FrequencyGain = 2.0f;
        }

        // Manager'da ayarladığın deprem süresi boyunca bekle
        yield return new WaitForSeconds(duration);

        // Süre bitince titremeyi tamamen sıfırla
        if (perlinComponent != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopSFX("Earthquake");
                AudioManager.Instance.StopSFX("Eathquake");
            }
            perlinComponent.AmplitudeGain = 0f;
            perlinComponent.FrequencyGain = 0f;
        }

        Debug.Log(">>> DEPREM BİTTİ.");
    }
}