using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] InteractableData data; // ScriptableObject data

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
        // Eğer etkileşime kapalıysa işlem yapma
        if (!isInteractable) return;

        ResolveHazard();
    }

    private void ResolveHazard()
    {
        isInteractable = false;

        if (data != null)
            Debug.Log(data.objectName + " ile etkileşime girildi!");
        else
            Debug.Log("Obje ile etkileşime girildi!");

        // Eğer sahnede görseller değişecekse önce onları kapatabilirsin
        if (activeVisual != null) activeVisual.SetActive(false);
        if (resolvedVisual != null) resolvedVisual.SetActive(true);

        // İstediğin gibi basınca objeyi tamamen yok eder (sahneden siler)
        Destroy(gameObject);
    }
}