using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Hayat üçgeni" güvenli alanları.
/// Bütün görevler bitince aday noktalardan rastgele 'zonesToShow' tanesi yerde sarı, yarı saydam kare olarak belirir.
/// Geri sayım bittiğinde oyuncu bu karelerden birinin içindeyse depremden sağ çıkar.
/// Aday noktalar bu objenin çocukları (veya 'spots' listesi) olarak sahnede durur; Scene görünümünde sarı gizmo ile görünür.
/// </summary>
public class SafeZoneManager : MonoBehaviour
{
    public static SafeZoneManager Instance { get; private set; }

    [Header("Candidate spots (life triangle positions)")]
    [Tooltip("Boş bırakılırsa bu objenin bütün çocukları aday nokta olarak kullanılır.")]
    [SerializeField] private List<Transform> spots = new List<Transform>();
    [SerializeField] private int zonesToShow = 2;

    [Header("Look")]
    [SerializeField] private Vector2 zoneSize = new Vector2(1.3f, 1.3f);
    [SerializeField] private Material zoneFillMaterial;
    [SerializeField] private Material zoneEdgeMaterial;
    [SerializeField] private float edgeWidth = 0.06f;
    [SerializeField] private float pulseSpeed = 3f;

    [Header("Detection")]
    [Tooltip("Oyuncunun merkezinin kareye ne kadar yakın olması gerektiği (kenar toleransı, metre)")]
    [SerializeField] private float tolerance = 0.15f;

    private readonly List<GameObject> activeZones = new List<GameObject>();
    private readonly List<Transform> activeSpots = new List<Transform>();
    private readonly List<Renderer> fillRenderers = new List<Renderer>();
    private MaterialPropertyBlock block;
    private Transform player;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public bool ZonesVisible => activeZones.Count > 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (spots == null) spots = new List<Transform>();
        if (spots.Count == 0)
        {
            foreach (Transform child in transform) spots.Add(child);
        }
        block = new MaterialPropertyBlock();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowZones()
    {
        HideZones();
        if (spots.Count == 0) return;

        List<Transform> pool = new List<Transform>(spots);
        // Sağlık Kiti alındıysa fazladan güvenli alan çıkar
        int count = Mathf.Clamp(zonesToShow + ItemEffectsManager.ExtraSafeZones, 1, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            Transform spot = pool[index];
            pool.RemoveAt(index);
            if (spot == null) continue;
            activeSpots.Add(spot);
            activeZones.Add(BuildZoneVisual(spot));
        }
    }

    public void HideZones()
    {
        foreach (GameObject z in activeZones)
        {
            if (z != null) Destroy(z);
        }
        activeZones.Clear();
        activeSpots.Clear();
        fillRenderers.Clear();
    }

    public bool IsPlayerInsideAnyZone()
    {
        Transform p = GetPlayer();
        if (p == null) return false;
        foreach (Transform spot in activeSpots)
        {
            if (spot != null && IsInside(spot, p.position)) return true;
        }
        return false;
    }

    private bool IsInside(Transform spot, Vector3 worldPos)
    {
        Vector3 local = Quaternion.Inverse(spot.rotation) * (worldPos - spot.position);
        return Mathf.Abs(local.x) <= zoneSize.x * 0.5f + tolerance &&
               Mathf.Abs(local.z) <= zoneSize.y * 0.5f + tolerance &&
               local.y > -1f && local.y < 2.5f;
    }

    private Transform GetPlayer()
    {
        if (player == null)
        {
            GameObject go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }
        return player;
    }

    private void Update()
    {
        if (activeZones.Count == 0) return;

        bool inside = IsPlayerInsideAnyZone();
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * (inside ? 2f : 1f));
        foreach (Renderer r in fillRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;
            Color c = r.sharedMaterial.HasProperty(BaseColorId) ? r.sharedMaterial.GetColor(BaseColorId) : Color.yellow;
            c.a = Mathf.Clamp01(c.a * (inside ? 1.6f : 0.7f + 0.6f * pulse));
            r.GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            r.SetPropertyBlock(block);
        }
    }

    private GameObject BuildZoneVisual(Transform spot)
    {
        GameObject root = new GameObject("Safe Zone (" + spot.name + ")");
        root.transform.SetPositionAndRotation(spot.position, spot.rotation);

        GameObject fill = CreatePart("Fill", root.transform, new Vector3(0f, 0.012f, 0f),
            new Vector3(zoneSize.x, 0.01f, zoneSize.y), zoneFillMaterial);
        Renderer fr = fill.GetComponent<Renderer>();
        if (fr != null)
        {
            fr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            fr.receiveShadows = false;
            fillRenderers.Add(fr);
        }

        float hx = zoneSize.x * 0.5f, hz = zoneSize.y * 0.5f;
        CreatePart("Edge", root.transform, new Vector3(0f, 0.016f, hz), new Vector3(zoneSize.x + edgeWidth, 0.012f, edgeWidth), zoneEdgeMaterial);
        CreatePart("Edge", root.transform, new Vector3(0f, 0.016f, -hz), new Vector3(zoneSize.x + edgeWidth, 0.012f, edgeWidth), zoneEdgeMaterial);
        CreatePart("Edge", root.transform, new Vector3(hx, 0.016f, 0f), new Vector3(edgeWidth, 0.012f, zoneSize.y), zoneEdgeMaterial);
        CreatePart("Edge", root.transform, new Vector3(-hx, 0.016f, 0f), new Vector3(edgeWidth, 0.012f, zoneSize.y), zoneEdgeMaterial);

        // Uzaktan da görülsün diye yukarı doğru hafif bir ışık sütunu
        GameObject beam = CreatePart("Beam", root.transform, new Vector3(0f, 1.1f, 0f),
            new Vector3(zoneSize.x * 0.98f, 2.2f, zoneSize.y * 0.98f), zoneFillMaterial);
        Renderer br = beam.GetComponent<Renderer>();
        if (br != null)
        {
            br.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            br.receiveShadows = false;
            br.GetPropertyBlock(block);
            Color c = zoneFillMaterial != null && zoneFillMaterial.HasProperty(BaseColorId) ? zoneFillMaterial.GetColor(BaseColorId) : new Color(1f, 0.85f, 0.1f, 0.3f);
            c.a *= 0.25f;
            block.SetColor(BaseColorId, c);
            br.SetPropertyBlock(block);
        }
        return root;
    }

    private static GameObject CreatePart(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Collider col = go.GetComponent<Collider>();
        if (col != null) Destroy(col); // Görsel sadece; oyuncuyu engellemesin, raycast'e takılmasın
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null && mat != null) r.sharedMaterial = mat;
        return go;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.6f);
        List<Transform> list = (spots != null && spots.Count > 0) ? spots : null;
        if (list == null)
        {
            foreach (Transform child in transform)
                Gizmos.DrawWireCube(child.position + Vector3.up * 0.05f, new Vector3(zoneSize.x, 0.1f, zoneSize.y));
            return;
        }
        foreach (Transform t in list)
        {
            if (t != null) Gizmos.DrawWireCube(t.position + Vector3.up * 0.05f, new Vector3(zoneSize.x, 0.1f, zoneSize.y));
        }
    }
}
