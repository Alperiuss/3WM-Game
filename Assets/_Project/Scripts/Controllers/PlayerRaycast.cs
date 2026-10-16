using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input System kütüphanesi

public class PlayerRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPromptUI;

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactActionReference; // Input Action referansı

    private void OnEnable()
    {
        if (interactActionReference != null)
            interactActionReference.action.Enable();
    }

    private void OnDisable()
    {
        if (interactActionReference != null)
            interactActionReference.action.Disable();
    }

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool foundInteractable = false;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<InteractableObject>();
            }

            if (interactable != null && interactable.IsInteractable)
            {
                foundInteractable = true;

                // Yeni Input Sistemi üzerinden Interact tuşuna (E veya Gamepad) basıldı mı?
                if (interactActionReference != null && interactActionReference.action.WasPressedThisFrame())
                {
                    interactable.Interact();
                }
            }
        }

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(foundInteractable);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * interactionDistance);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hit.point, Vector3.one * 0.1f);
        }
    }
}