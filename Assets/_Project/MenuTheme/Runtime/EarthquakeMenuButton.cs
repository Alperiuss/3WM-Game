using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NotYetSafe.MenuTheme
{
    /// <summary>Subtle hover and press feedback for the existing native Buttons.</summary>
    [AddComponentMenu("Not Yet Safe/Menu/Button Motion")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class EarthquakeMenuButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private Vector3 initialScale;
        private bool hovered;
        private bool selected;
        private bool pressed;
        private Button button;

        private void Awake()
        {
            initialScale = transform.localScale;
            button = GetComponent<Button>();
        }

        private void Update()
        {
            bool usable = button != null && button.IsInteractable();
            float amount = usable && pressed ? 0.985f :
                usable && (hovered || selected) ? 1.012f : 1f;
            float blend = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale,
                initialScale * amount, blend);
        }

        private void OnDisable()
        {
            hovered = selected = pressed = false;
            transform.localScale = initialScale;
        }

        public void OnPointerEnter(PointerEventData eventData) { hovered = true; }
        public void OnPointerExit(PointerEventData eventData) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                pressed = true;
        }
        public void OnPointerUp(PointerEventData eventData) { pressed = false; }
        public void OnSelect(BaseEventData eventData) { selected = true; }
        public void OnDeselect(BaseEventData eventData) { selected = false; pressed = false; }
    }
}
