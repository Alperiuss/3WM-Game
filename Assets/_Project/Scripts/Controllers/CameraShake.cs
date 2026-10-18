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
                AudioManager.Instance.PlaySFX("Earthquake");
                
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
            if (AudioManager.Instance != null && AudioManager.Instance.IsPlayingSFX("Earthquake"))
            {
                AudioManager.Instance.StopSFX("Earthquake");
            }
            perlinComponent.AmplitudeGain = 0f;
            perlinComponent.FrequencyGain = 0f;
        }

        Debug.Log(">>> DEPREM BİTTİ.");
    }
}