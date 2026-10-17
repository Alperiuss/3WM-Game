using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveRadioSystem : MonoBehaviour
{
    public static WaveRadioSystem Instance { get; private set; }

    [System.Serializable]
    public class WaveBroadcast
    {
        public int waveNumber; // Hangi bölüm sonrası çalışacak (Örn: 1, 2, 3...)
        
        [Header("Timing Settings (Per Wave)")]
        public float typingSpeed = 0.04f;   // Bu wave'e özel harf yazma hızı
        public float displayDuration = 3.5f; // Bu wave'e özel cümlede bekleme süresi

        [TextArea(2, 4)]
        public string[] messages; // O bölüm sonunda çıkacak parça parça metinler
    }

    [Header("Wave Broadcast Data")]
    [SerializeField] private List<WaveBroadcast> waveBroadcastList = new List<WaveBroadcast>();

    [Header("UI References")]
    [SerializeField] private GameObject radioPanel;
    [SerializeField] private TMP_Text radioText;

    private Coroutine currentRadioCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (radioPanel != null)
        {
            radioPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // KLAVYEDEN TEST ETMEK İÇİN (Sayı Tuşları 1-5):
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnWaveCompleted(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnWaveCompleted(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnWaveCompleted(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) OnWaveCompleted(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) OnWaveCompleted(5);
    }

    // BÖLÜM BİTTİĞİNDE ÇAĞRILACAK METOT
    public void OnWaveCompleted(int waveIndex)
    {
        WaveBroadcast broadcast = waveBroadcastList.Find(b => b.waveNumber == waveIndex);

        if (broadcast != null && broadcast.messages.Length > 0)
        {
            PlayRadioBroadcast(broadcast);
        }
        else
        {
            Debug.LogWarning($"[RadioSystem] Bölüm {waveIndex} için radyo mesajı bulunamadı!");
        }
    }

    private void PlayRadioBroadcast(WaveBroadcast broadcast)
    {
        if (currentRadioCoroutine != null)
        {
            StopCoroutine(currentRadioCoroutine);
        }

        currentRadioCoroutine = StartCoroutine(BroadcastRoutine(broadcast));
    }

    private IEnumerator BroadcastRoutine(WaveBroadcast broadcast)
    {
        radioPanel.SetActive(true);

        foreach (string message in broadcast.messages)
        {
            radioText.text = "";

            // O wave'e özel daktilo hızı (typingSpeed) kullanılır
            foreach (char letter in message.ToCharArray())
            {
                radioText.text += letter;
                yield return new WaitForSeconds(broadcast.typingSpeed);
            }

            // O wave'e özel bekleme süresi (displayDuration) kullanılır
            yield return new WaitForSeconds(broadcast.displayDuration);
        }

        radioText.text = "";
        radioPanel.SetActive(false);
        currentRadioCoroutine = null;
    }
}