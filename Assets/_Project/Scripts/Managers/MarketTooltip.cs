using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Market kartlarının üzerine gelince açılan açıklama kutusu (başlık, ne işe yaradığı, fiyat/durum).
/// Kart ile aynı canvas içinde kod ile kurulur ve fareyi takip eder; ekran dışına taşmaz.
/// </summary>
public class MarketTooltip : MonoBehaviour
{
    private static MarketTooltip instance;

    private const float Width = 470f;
    private static readonly Vector2 CursorOffset = new Vector2(24f, -24f);

    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform rect;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private TMP_Text footerText;

    public static void Show(Component source, string title, string body, string footer, TMP_FontAsset font)
    {
        if (source == null) return;
        Canvas parentCanvas = source.GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;
        Canvas root = parentCanvas.rootCanvas;

        if (instance == null || instance.canvas != root)
        {
            Hide();
            instance = Create(root, font);
        }

        instance.titleText.text = title;
        instance.bodyText.text = body;
        instance.footerText.text = footer;
        instance.footerText.gameObject.SetActive(!string.IsNullOrEmpty(footer));
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(instance.rect);
        instance.FollowCursor();
    }

    public static void Hide()
    {
        if (instance != null) instance.gameObject.SetActive(false);
    }

    private static MarketTooltip Create(Canvas root, TMP_FontAsset font)
    {
        GameObject go = new GameObject("MarketTooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(root.transform, false);

        MarketTooltip tip = go.AddComponent<MarketTooltip>();
        tip.canvas = root;
        tip.canvasRect = (RectTransform)root.transform;
        tip.rect = (RectTransform)go.transform;
        tip.rect.pivot = new Vector2(0f, 1f);
        tip.rect.anchorMin = tip.rect.anchorMax = new Vector2(0.5f, 0.5f);
        tip.rect.sizeDelta = new Vector2(Width, 0f);

        Image bg = go.GetComponent<Image>();
        bg.color = new Color(0.06f, 0.09f, 0.13f, 0.95f);
        bg.raycastTarget = false;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.78f, 0.1f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        CanvasGroup group = go.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 18, 18);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        tip.titleText = CreateText("Title", go.transform, 34f, new Color(1f, 0.78f, 0.1f, 1f), font);
        tip.bodyText = CreateText("Description", go.transform, 26f, new Color(0.93f, 0.93f, 0.9f, 1f), font);
        tip.footerText = CreateText("Price", go.transform, 26f, new Color(0.55f, 0.85f, 1f, 1f), font);
        return tip;
    }

    private static TMP_Text CreateText(string name, Transform parent, float size, Color color, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private void Update()
    {
        FollowCursor();
    }

    private void FollowCursor()
    {
        Vector2 screen = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out Vector2 local)) return;

        Vector2 size = rect.rect.size;
        Rect bounds = canvasRect.rect;
        Vector2 pos = local + CursorOffset;

        // Sağa veya aşağı taşacaksa imlecin diğer tarafına geç
        if (pos.x + size.x > bounds.xMax) pos.x = local.x - CursorOffset.x - size.x;
        if (pos.y - size.y < bounds.yMin) pos.y = local.y - CursorOffset.y + size.y;
        pos.x = Mathf.Clamp(pos.x, bounds.xMin, bounds.xMax - size.x);
        pos.y = Mathf.Clamp(pos.y, bounds.yMin + size.y, bounds.yMax);

        rect.anchoredPosition = pos;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
