using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum DeathReason
{
    Stress,
    NoCover
}

/// <summary>
/// Kaybedince açılan "MISSION UNSUCCESFUL" not defteri ekranı.
/// UI kod ile kurulur (sprite ve font Inspector'dan gelir), tek buton: sol altta MAIN MENU.
/// Ana menüye dönünce bütün ilerleme (para, satın alımlar, gün) sıfırlanır.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    [Header("Art (same as the summary / menu)")]
    [SerializeField] private Sprite backgroundSprite;   // Frame_4_7 (mavi sismik desen)
    [SerializeField] private Sprite clipboardSprite;    // sekreterlik (MISSION UNSUCCESFUL)
    [SerializeField] private Sprite victoryClipboardSprite; // sekreterlik_1 (MISSION SUCCESFUL)
    [SerializeField] private Sprite buttonSprite;       // Frame_2_3
    [SerializeField] private TMP_FontAsset font;        // STaRTp2 SDF

    [Header("Flow")]
    [SerializeField] private string mainMenuSceneName = "MenuScene_Deprem";
    [SerializeField] private float fadeInDuration = 0.8f;

    [Header("Texts (English)")]
    [SerializeField] private string stressTitle = "YOU PANICKED";
    [TextArea(2, 4)]
    [SerializeField] private string stressBody = "Your stress level hit the limit.\nPanic took over before the earthquake.";
    [SerializeField] private string coverTitle = "YOU DIDN'T TAKE COVER";
    [TextArea(2, 4)]
    [SerializeField] private string coverBody = "You weren't in a safe spot (life triangle)\nwhen the earthquake hit.";
    [TextArea(1, 3)]
    [SerializeField] private string unsecuredExtra = "Some hazards in the house were still unsecured.";

    [Header("Victory Texts (English)")]
    [SerializeField] private string victoryTitle = "CONGRATULATIONS!";
    [TextArea(2, 5)]
    [SerializeField] private string victoryBody = "You survived every earthquake and kept your family safe.\nYou secured your home, kept calm and took cover in time.\n\nYour home is NOT YET SAFE... but you are ready.";

    private GameObject canvasRoot;
    private CanvasGroup group;
    private bool shown;

    public bool IsShown => shown;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(DeathReason reason, bool tasksWereIncomplete, int day)
    {
        if (shown) return;
        shown = true;

        string title = reason == DeathReason.Stress ? stressTitle : coverTitle;
        string body = reason == DeathReason.Stress ? stressBody : coverBody;
        if (reason == DeathReason.NoCover && tasksWereIncomplete && !string.IsNullOrEmpty(unsecuredExtra))
            body += "\n\n" + unsecuredExtra;

        BuildUI(clipboardSprite, "MISSION UNSUCCESFUL", new Color(0.85f, 0.05f, 0.05f, 1f),
                title, new Color(0.08f, 0.08f, 0.1f, 1f), body, "SURVIVED DAYS: " + Mathf.Max(0, day - 1));
        Open();
    }

    /// <summary>Son gün de atlatılınca açılan tebrik ekranı (market yerine). MAIN MENU ana menüye döner.</summary>
    public void ShowVictory(int survivedDays)
    {
        if (shown) return;
        shown = true;

        BuildUI(victoryClipboardSprite, "MISSION SUCCESFUL", new Color(0.36f, 0.62f, 0.13f, 1f),
                victoryTitle, new Color(0.13f, 0.45f, 0.12f, 1f), victoryBody, "SURVIVED DAYS: " + survivedDays + " / " + survivedDays);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Coins");
        Open();
    }

    private void Open()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            if (group != null) group.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

    private void BuildUI(Sprite boardSprite, string fallbackHeader, Color headerColor,
                         string title, Color titleColor, string body, string footer)
    {
        EnsureEventSystem();

        canvasRoot = new GameObject("GameOverCanvas", typeof(RectTransform));
        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRoot.AddComponent<GraphicRaycaster>();
        group = canvasRoot.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        // Arka plan (koyu zemin + sismik desen)
        Image dark = CreateImage("Dim", canvasRoot.transform, null, new Color(0.043f, 0.075f, 0.114f, 1f));
        Stretch(dark.rectTransform);
        if (backgroundSprite != null)
        {
            Image bg = CreateImage("Background", canvasRoot.transform, backgroundSprite, Color.white);
            Stretch(bg.rectTransform);
            bg.preserveAspect = false;
        }

        // Not defteri / klipsli pano
        Image board = CreateImage("Clipboard", canvasRoot.transform, boardSprite,
            boardSprite != null ? Color.white : new Color(0.85f, 0.82f, 0.74f, 1f));
        board.preserveAspect = true;
        RectTransform br = board.rectTransform;
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
        br.pivot = new Vector2(0.5f, 0.5f);
        br.sizeDelta = new Vector2(1000f, 823f); // 4558x3750 oranı
        br.anchoredPosition = new Vector2(0f, 10f);

        if (boardSprite == null)
        {
            TMP_Text header = CreateText("Header", board.transform, fallbackHeader, 64, headerColor);
            Place(header.rectTransform, new Vector2(0f, 300f), new Vector2(900f, 90f));
        }

        TMP_Text titleText = CreateText("Title", board.transform, title, 54, titleColor);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 30f;
        titleText.fontSizeMax = 54f;
        Place(titleText.rectTransform, new Vector2(0f, 120f), new Vector2(860f, 90f));

        TMP_Text bodyText = CreateText("Body", board.transform, body, 34, new Color(0.12f, 0.12f, 0.14f, 1f));
        bodyText.alignment = TextAlignmentOptions.Top;
        bodyText.enableAutoSizing = true;
        bodyText.fontSizeMin = 20f;
        bodyText.fontSizeMax = 34f;
        Place(bodyText.rectTransform, new Vector2(0f, -70f), new Vector2(820f, 260f));

        TMP_Text dayText = CreateText("Day", board.transform, footer, 30, new Color(0.25f, 0.25f, 0.28f, 1f));
        Place(dayText.rectTransform, new Vector2(0f, -280f), new Vector2(820f, 50f));

        // Sol altta ANA MENÜ butonu
        Image btnImage = CreateImage("MainMenuButton", canvasRoot.transform, buttonSprite,
            buttonSprite != null ? Color.white : new Color(0.78f, 0.78f, 0.72f, 1f));
        RectTransform bt = btnImage.rectTransform;
        bt.anchorMin = bt.anchorMax = new Vector2(0f, 0f);
        bt.pivot = new Vector2(0f, 0f);
        bt.sizeDelta = new Vector2(440f, 98f);
        bt.anchoredPosition = new Vector2(60f, 50f);
        Button button = btnImage.gameObject.AddComponent<Button>();
        ColorBlock cb = button.colors;
        cb.highlightedColor = new Color(1f, 0.93f, 0.8f, 1f);
        cb.pressedColor = new Color(0.8f, 0.75f, 0.65f, 1f);
        button.colors = cb;
        button.targetGraphic = btnImage;
        button.onClick.AddListener(OnMainMenuPressed);

        TMP_Text btnText = CreateText("Text", btnImage.transform, "MAIN MENU", 40, new Color(0.1f, 0.1f, 0.12f, 1f));
        Stretch(btnText.rectTransform);
        btnText.rectTransform.offsetMin = new Vector2(10f, 14f);
        btnText.rectTransform.offsetMax = new Vector2(-10f, -6f);

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void OnMainMenuPressed()
    {
        ResetRunProgress();

        Time.timeScale = 1f;

        // DayManager DontDestroyOnLoad; yeni oyunda tekrar temiz kurulsun diye yok ediyoruz
        if (DayManager.Instance != null) Destroy(DayManager.Instance.gameObject);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>Kaybeden baştan başlar: para, satın alımlar ve seviyeler sıfırlanır (ses/ekran ayarları korunur).</summary>
    public static void ResetRunProgress()
    {
        PlayerPrefs.DeleteKey("Player_Fame");
        ItemCard[] cards = Resources.FindObjectsOfTypeAll<ItemCard>();
        foreach (ItemCard card in cards)
        {
            if (card == null || string.IsNullOrEmpty(card.ItemID)) continue;
            PlayerPrefs.DeleteKey("Bought_" + card.ItemID);
            PlayerPrefs.DeleteKey("Level_" + card.ItemID);
            PlayerPrefs.DeleteKey("PurchasedItem_" + card.ItemID);
            PlayerPrefs.DeleteKey("PurchasedItem_" + card.ItemID + "_Level");
        }
        PlayerPrefs.Save();
    }

    // ---------------------------------------------------------------- UI helpers
    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = true;
        return img;
    }

    private TMP_Text CreateText(string name, Transform parent, string text, float size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
