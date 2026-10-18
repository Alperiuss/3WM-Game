using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tehlike göstergesi (Alarm eşyası ve 1. gün rehberi): hedefin üstünde nabız gibi atan bir işaret ve mesafe yazısı.
/// Hedef ekran dışındaysa işaret ekranın kenarına yapışır ve yönü gösterir.
/// Aynı anda birden fazla hedef işaretlenebilir. UI tamamen kod ile kurulur; hedef çözülünce işareti kaybolur.
/// </summary>
public class ItemLocatorMarker : MonoBehaviour
{
    private static readonly List<ItemLocatorMarker> actives = new List<ItemLocatorMarker>();

    private const float EdgeMargin = 80f;
    private static readonly Color MarkerColor = new Color(1f, 0.78f, 0.1f, 1f);

    private InteractableObject target;
    private Vector3 targetOffset;
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform markerRoot;
    private RectTransform diamond;
    private TMP_Text distanceText;

    /// <summary>Hedefi işaretler (önceki işaretler kalır).</summary>
    public static void Show(InteractableObject target, TMP_FontAsset font)
    {
        if (target == null) return;
        foreach (ItemLocatorMarker m in actives)
        {
            if (m != null && m.target == target) return;
        }

        GameObject go = new GameObject("HazardMarker (" + target.name + ")");
        ItemLocatorMarker marker = go.AddComponent<ItemLocatorMarker>();
        marker.Build(target, font);
        actives.Add(marker);
    }

    /// <summary>Bütün işaretleri kaldırır.</summary>
    public static void Hide()
    {
        foreach (ItemLocatorMarker m in actives)
        {
            if (m != null) Destroy(m.gameObject);
        }
        actives.Clear();
    }

    private void Remove()
    {
        actives.Remove(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        actives.Remove(this);
    }

    private void Build(InteractableObject markedTarget, TMP_FontAsset font)
    {
        target = markedTarget;
        targetOffset = CalculateTopOffset(markedTarget.transform);

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect = (RectTransform)transform;

        markerRoot = new GameObject("Marker", typeof(RectTransform)).GetComponent<RectTransform>();
        markerRoot.SetParent(transform, false);
        markerRoot.sizeDelta = new Vector2(140f, 120f);

        // Döndürülmüş kare = elmas işaret
        diamond = CreateImage("Diamond", markerRoot, new Vector2(54f, 54f), new Color(0.08f, 0.08f, 0.1f, 0.85f));
        diamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
        RectTransform inner = CreateImage("Fill", diamond, new Vector2(44f, 44f), MarkerColor);
        inner.localRotation = Quaternion.identity;

        TMP_Text bang = CreateText("Icon", markerRoot, "!", 40f, new Color(0.1f, 0.08f, 0.05f, 1f), font);
        bang.rectTransform.sizeDelta = new Vector2(54f, 54f);

        distanceText = CreateText("Distance", markerRoot, "", 26f, MarkerColor, font);
        distanceText.rectTransform.anchoredPosition = new Vector2(0f, -50f);
        distanceText.rectTransform.sizeDelta = new Vector2(160f, 34f);
        distanceText.enableAutoSizing = true;
        distanceText.fontSizeMin = 14f;
        distanceText.fontSizeMax = 26f;
        distanceText.outlineWidth = 0.2f;
        distanceText.outlineColor = new Color32(0, 0, 0, 255);
    }

    private static Vector3 CalculateTopOffset(Transform t)
    {
        Renderer[] renderers = t.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return Vector3.up * 0.6f;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return new Vector3(b.center.x - t.position.x, b.max.y - t.position.y + 0.35f, b.center.z - t.position.z);
    }

    private void LateUpdate()
    {
        if (target == null || !target.IsInteractable)
        {
            Remove();
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            markerRoot.gameObject.SetActive(false);
            return;
        }
        markerRoot.gameObject.SetActive(true);

        Vector3 worldPos = target.transform.position + targetOffset;
        Vector3 sp = cam.WorldToScreenPoint(worldPos);
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 p = new Vector2(sp.x, sp.y);

        bool behind = sp.z < 0f;
        if (behind) p = center - (p - center);

        bool onScreen = !behind &&
                        p.x >= EdgeMargin && p.x <= Screen.width - EdgeMargin &&
                        p.y >= EdgeMargin && p.y <= Screen.height - EdgeMargin;
        if (!onScreen)
        {
            // Ekran kenarına yapıştır (hedefin yönünde)
            Vector2 dir = p - center;
            if (dir.sqrMagnitude < 0.01f) dir = Vector2.down;
            float sx = (center.x - EdgeMargin) / Mathf.Max(0.001f, Mathf.Abs(dir.x));
            float sy = (center.y - EdgeMargin) / Mathf.Max(0.001f, Mathf.Abs(dir.y));
            p = center + dir * Mathf.Min(sx, sy);
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, p, null, out Vector2 local))
        {
            markerRoot.anchoredPosition = local;
        }

        float pulse = 1f + 0.12f * Mathf.Sin(Time.unscaledTime * 6f);
        diamond.localScale = Vector3.one * pulse;

        float distance = Vector3.Distance(cam.transform.position, target.transform.position);
        distanceText.text = Mathf.RoundToInt(distance) + "m";
    }

    private static RectTransform CreateImage(string name, Transform parent, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        RectTransform rt = (RectTransform)go.transform;
        rt.sizeDelta = size;
        return rt;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float size, Color color, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }
}
