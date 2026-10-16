using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] float interactionDistance = 3f;
    [SerializeField] LayerMask interactableLayer;

    [Header("UI Settings")]
    [SerializeField] GameObject interactionPromptUI;

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool foundInteractable = false;

        // Işın bir şeye çarpıyor mu?
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<InteractableObject>();
            }

            // Geçerli ve etkileşime açık bir objeye bakıyorsak
            if (interactable != null && interactable.IsInteractable)
            {
                foundInteractable = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
        }

        // UI durumunu her karede net olarak güncelle (Bakıyorsan açık, bakmıyorsan kapalı)
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(foundInteractable);
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos rengini sarı yap
        Gizmos.color = Color.yellow;

        // Işının başlangıç ve bitiş noktasını çiz
        Gizmos.DrawRay(transform.position, transform.forward * interactionDistance);

        // Eğer bir şeye çarptıysa oraya küçük bir küp çiz
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            Gizmos.color = Color.red; // Çarpınca kırmızı
            Gizmos.DrawWireCube(hit.point, Vector3.one * 0.1f);
        }
    }
}