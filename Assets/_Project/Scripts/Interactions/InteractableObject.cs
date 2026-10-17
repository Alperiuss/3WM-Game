using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] InteractableData data;

    [Header("Conditions")]
    [SerializeField] bool isInteractable = true;
    [SerializeField] bool isHazardous = false;

    [Header("Visual & Color References")]
    [SerializeField] private Renderer targetRenderer; // Rengi değişecek olan gerçek mesh objesini buraya sürükleyeceğiz
    [SerializeField] GameObject activeVisual;
    [SerializeField] GameObject resolvedVisual;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color interactedColor = Color.green;

    public bool IsInteractable
    {
        get => isInteractable;
        set => isInteractable = value;
    }

    private void Start()
    {
        // Başlangıçta objenin normal rengini ayarla
        SetObjectColor(normalColor);

        if (activeVisual != null) activeVisual.SetActive(true);
        if (resolvedVisual != null) resolvedVisual.SetActive(false);
    }

    public void Interact()
    {
        Debug.Log(">>> Interact() fonksiyonu çalıştı!");

        if (!isInteractable)
        {
            Debug.Log(">>> HATA: isInteractable false olduğu için işlem yapılmadı!");
            return;
        }

        ResolveHazard();
    }

    private void ResolveHazard()
    {
        Debug.Log(">>> ResolveHazard() çalıştı, obje yok ediliyor!");
        isInteractable = false;

        SetObjectColor(interactedColor);

        if (activeVisual != null) activeVisual.SetActive(false);
        if (resolvedVisual != null) resolvedVisual.SetActive(true);

    }

    private void SetObjectColor(Color targetColor)
    {
        if (targetRenderer != null)
        {
            // Materyalin rengini değiştirir
            targetRenderer.material.color = targetColor;
        }
    }
}