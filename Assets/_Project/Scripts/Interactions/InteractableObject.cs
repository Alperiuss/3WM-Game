using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] InteractableData data;

    [Header("Conditions")]
    [SerializeField] private bool isInteractable = false;

    [Header("Visual & Color References")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] GameObject activeVisual;
    [SerializeField] GameObject resolvedVisual;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.red;     // Aktifken (Kırmızı)
    [SerializeField] private Color disabledColor = Color.gray;  // Pasifken (Gri)
    [SerializeField] private Color interactedColor = Color.green; // Çözüldüğünde (Yeşil)

    private bool isResolved = false;

    public bool IsInteractable => isInteractable && !isResolved;

    private void Start()
    {
        // Başlangıçta nesnenin rengini ayarla
        SetObjectColor(isInteractable ? normalColor : disabledColor);

        if (activeVisual != null) activeVisual.SetActive(isInteractable);
        if (resolvedVisual != null) resolvedVisual.SetActive(false);
    }

    // GameManager bu fonksiyonu çağırarak nesnenin aktif/pasif durumunu belirleyecek
    public void SetInteractableState(bool state)
    {
        isInteractable = state;
        isResolved = false; // Yeni turda çözülme durumunu sıfırla

        if (targetRenderer != null)
        {
            targetRenderer.material.color = isInteractable ? normalColor : disabledColor;
        }

        if (activeVisual != null) activeVisual.SetActive(isInteractable);
        if (resolvedVisual != null) resolvedVisual.SetActive(false);
    }

    public void Interact()
    {
        if (!IsInteractable) return;

        ResolveHazard();
    }

    private void ResolveHazard()
    {
        isResolved = true;
        SetObjectColor(interactedColor);

        Debug.Log("Obje ile etkileşime girildi, renk yeşile döndü!");

        if (InteractableRandomChance.Instance != null)
            InteractableRandomChance.Instance.NotifyObjectResolved();


        if (activeVisual != null) activeVisual.SetActive(false);
        if (resolvedVisual != null) resolvedVisual.SetActive(true);
    }

    private void SetObjectColor(Color targetColor)
    {
        if (targetRenderer != null)
        {
            targetRenderer.material.color = targetColor;
        }
    }
}