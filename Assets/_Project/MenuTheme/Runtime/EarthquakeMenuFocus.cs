using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NotYetSafe.MenuTheme
{
    [AddComponentMenu("Not Yet Safe/Menu/Panel Focus")]
    [DisallowMultipleComponent]
    public sealed class EarthquakeMenuFocus : MonoBehaviour
    {
        [SerializeField] private Button defaultButton;

        private void OnEnable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            StartCoroutine(SelectAfterLayout());
        }

        private IEnumerator SelectAfterLayout()
        {
            // The EventSystem and the menu's existing Start methods must run first.
            yield return null;
            if (isActiveAndEnabled && defaultButton != null &&
                defaultButton.IsActive() && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
            }
        }
    }
}
