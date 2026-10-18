using UnityEngine;

public class LobbyMusicStarter : MonoBehaviour
{
    [Header("Lobi Müziği")]
    [Tooltip("Çalmasını istediğin müzik dosyasını (.mp3 veya .wav) buraya sürükle")]
    [SerializeField] private AudioClip lobbyMusicClip;

    private void Start()
    {
        // Lobi sahnesi açıldığı an müziği döngüde (Loop) başlatır
        if (lobbyMusicClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(lobbyMusicClip);
        }
    }

    /// <summary>
    /// Oyunu Başlat (Play) butonuna basıldığında lobi müziğini durdurmak istersen:
    /// </summary>
    public void StopLobbyMusic()
    {
        if (AudioManager.Instance != null)
        {
            // İstersen müziği durdurabilirsin
            // AudioManager.Instance.PlayMusic(null);
        }
    }
}
