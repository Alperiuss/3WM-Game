using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input System kütüphanesi
using UnityEngine.UI; // UI için gerekli

public class PlayerRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [Tooltip("Duvar/mobilya gibi bakışı engelleyen katmanlar (eşyaya duvarın arkasından etkileşimi önler)")]
    [SerializeField] private LayerMask blockingLayers = 1; // Default

    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private Image progressRingImage; // İlerleme halkası için UI Image referansı

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactActionReference; // Input Action referansı

    private float currentHoldTime = 0f;
    private InteractableObject currentTarget;

    private void OnEnable()
    {
        if (interactActionReference != null && interactActionReference.action != null)
            interactActionReference.action.Enable();
        else
            Debug.LogWarning("[PlayerRaycast] Interact Action Reference atanmamış, etkileşim çalışmayacak.", this);
    }

    private void OnDisable()
    {
        if (interactActionReference != null && interactActionReference.action != null)
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
        InteractableObject hitInteractable = null;

        int mask = interactableLayer.value | blockingLayers.value;
        if (Physics.Raycast(ray, out hit, interactionDistance, mask, QueryTriggerInteraction.Ignore))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<InteractableObject>();
            }

            if (interactable != null && interactable.IsInteractable)
            {
                foundInteractable = true;
                hitInteractable = interactable;
            }
        }

        // Eğer baktığımız obje değiştiyse veya artık bakmıyorsak süreyi sıfırla
        if (currentTarget != hitInteractable)
        {
            currentTarget = hitInteractable;
            currentHoldTime = 0f;
        }

        // Eğer geçerli bir objeye bakıyorsak
        if (currentTarget != null)
        {
            // E tuşuna basılıyor mu? (Basılı tutma)
            if (interactActionReference != null && interactActionReference.action != null && interactActionReference.action.IsPressed())
            {
                // =============================================================
                // MATKAP / TAMİR SESİ: Zaten çalmıyorsa başlat (Üst üste binmez!)
                // =============================================================
                if (AudioManager.Instance != null && !AudioManager.Instance.IsPlayingSFX("drill"))
                {
                    AudioManager.Instance.PlaySFX("drill");
                }
                currentHoldTime += Time.deltaTime;
                // Süre dolduysa etkileşimi gerçekleştir (Tamir Çantası / Elektrik Bandı süreyi kısaltır)
                if (currentHoldTime >= GetEffectiveHoldTime(currentTarget))
                {
                    // Tamir bittiğinde matkap sesini HEMEN DURDUR
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.StopSFX("drill");
                    }
                    currentTarget.Interact();
                    currentHoldTime = 0f;
                    currentTarget = null;
                }
            }
            else
            {
                // Oyuncu parmağını tuştan çekerse sesi DURDUR
                if (AudioManager.Instance != null && AudioManager.Instance.IsPlayingSFX("drill"))
                {
                    AudioManager.Instance.StopSFX("drill");
                }
                currentHoldTime = 0f;
            }
        }
        else
        {
            // Objenin üzerinden imleci çekerse sesi DURDUR
            if (AudioManager.Instance != null && AudioManager.Instance.IsPlayingSFX("drill"))
            {
                AudioManager.Instance.StopSFX("drill");
            }
            currentHoldTime = 0f;
        }

        // Halkanın doluluk oranını ve görünürlüğünü yönet
        if (progressRingImage != null)
        {
            float holdTime = currentTarget != null ? GetEffectiveHoldTime(currentTarget) : 0f;
            if (holdTime > 0f)
            {
                progressRingImage.gameObject.SetActive(true);
                progressRingImage.fillAmount = currentHoldTime / holdTime;
            }
            else
            {
                progressRingImage.gameObject.SetActive(false);
                progressRingImage.fillAmount = 0f;
            }
        }

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(foundInteractable);
        }
    }

    private static float GetEffectiveHoldTime(InteractableObject target)
    {
        return target.RequiredHoldTime * ItemEffectsManager.RepairTimeMultiplier(target.Data);
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