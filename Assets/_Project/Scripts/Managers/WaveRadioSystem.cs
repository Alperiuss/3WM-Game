using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class WaveRadioSystem : MonoBehaviour
{
    public static WaveRadioSystem Instance { get; private set; }

    [System.Serializable]
    public class WaveBroadcast
    {
        public int waveNumber; // Hangi gün/bölüm sonrası çalışacak (Örn: 1, 2, 3...)

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
    private bool submitActionPressed = false; // Enter tuşuna basıldığını takip eden bayrak

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

        // Radyo paneli açıkken Enter tuşuna basıldığında tetiklenir
        if (radioPanel != null && radioPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                submitActionPressed = true;
            }
        }
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
            Debug.LogWarning($"[RadioSystem] Bölüm {waveIndex} için radyo mesajı bulunamadı! Doğrudan geçiliyor.");
            if (DayManager.Instance != null)
            {
                DayManager.Instance.OnRadioFinished();
            }
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

        // ⭐ EKRAN AÇILIRKEN OYUNCU KAÇIRMASIN DİYE MİNİK BİR GECİKME (Yarım saniye)
        yield return new WaitForSecondsRealtime(0.5f);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Alper");
        }

        foreach (string message in broadcast.messages)
        {
            radioText.text = "";
            submitActionPressed = false;

            // 1. Aşama: Harf harf daktilo yazma efekti
            foreach (char letter in message.ToCharArray())
            {
                if (submitActionPressed)
                {
                    radioText.text = message;
                    submitActionPressed = false;
                    break;
                }

                radioText.text += letter;
                yield return new WaitForSecondsRealtime(broadcast.typingSpeed);
            }

            submitActionPressed = false;

            // 2. Aşama: Cümlenin ekranda kalma/bekleme süresi
            float waitTimer = broadcast.displayDuration;
            while (waitTimer > 0f)
            {
                if (submitActionPressed)
                {
                    submitActionPressed = false;
                    break;
                }

                waitTimer -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
        if (AudioManager.Instance != null && AudioManager.Instance.IsPlayingSFX("Alper"))
        {
            AudioManager.Instance.StopSFX("Alper");
        }

        radioText.text = "";
        radioPanel.SetActive(false);
        currentRadioCoroutine = null;

        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnRadioFinished();
        }
    }
}