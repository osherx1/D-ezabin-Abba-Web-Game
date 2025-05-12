using TMPro;
using UnityEngine;

namespace UI
{
    public class PowerTooltip : MonoBehaviour
    {
        public static PowerTooltip Instance;

        [SerializeField] private CanvasGroup cg;
        [SerializeField] private RectTransform rt; // root rect
        [SerializeField] private TMP_Text label;
        [SerializeField] private Canvas canvas; // assign in Inspector

        private void Awake()
        {
            Instance = this;
            if (!canvas) canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        /// <summary>
        /// Show tooltip at *canvas‑space* position.
        /// The caller passes a point in the canvas's coordinate system.
        /// </summary>
        public void Show(string text, Vector2 canvasPos)
        {
            label.text = text;
            rt.anchoredPosition = canvasPos;
            cg.alpha = 1;
            cg.blocksRaycasts = true;
        }

        public void Hide()
        {
            cg.alpha = 0;
            cg.blocksRaycasts = false;
        }
    }
}