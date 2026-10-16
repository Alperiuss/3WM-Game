using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] InteractableData data;

    [Header("Conditions")]
    [SerializeField] bool isInteractable = true;
    [SerializeField] bool isHazardous = false;

    [Header("Visual/Condition References")]
    [SerializeField] GameObject activeVisual;
    [SerializeField] GameObject resolvedVisual;

    public bool IsInteractable
    {
        get => isInteractable;
        set => isInteractable = value;
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

        if (activeVisual != null) activeVisual.SetActive(false);
        if (resolvedVisual != null) resolvedVisual.SetActive(true);

        Destroy(gameObject);
    }
}