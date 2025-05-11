using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utilities;

namespace UI
{
    [RequireComponent(typeof(Button), typeof(Image))]
    public class CoinBoostButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        /* ──────────────── Inspector ──────────────── */
        [Header("Sprites")] [SerializeField] private Sprite readySprite; // normal icon
        [SerializeField] private Sprite disabledSprite; // greyed icon (optional)

        [Header("Overlay Image (Filled/Radial)")] [SerializeField]
        private Image overlay; // one image reused for both phases

        [Header("Timing")] [SerializeField] private float activeSeconds = 10f; // boost length
        [SerializeField] private float cooldownSeconds = 60f; // time to next use

        [Header("Overlay Colours")] [SerializeField]
        private Color activeColor = new(1f, 0.85f, 0f, 0.65f); // gold-ish

        [SerializeField] private Color cooldownColor = new(0f, 0f, 0f, 0.55f); // dark

        /* ──────────────── Internal ──────────────── */
        private enum Phase
        {
            Ready,
            Active,
            Cooldown
        }

        private Phase _phase = Phase.Ready;
        private Button _btn;
        private Image _img;


        private RectTransform _rect; // for tooltip positioning
        private float _timer; // counts down in Active / Cooldown

        /* ──────────────── Unity ──────────────── */
        private void Awake()
        {
            _btn = GetComponent<Button>();
            _img = GetComponent<Image>();
            _btn.onClick.AddListener(Activate);
            _rect = GetComponent<RectTransform>();
            
            overlay.gameObject.SetActive(false);
            _img.sprite = readySprite;
        }

        private void Update()
        {
            if (_phase == Phase.Ready) return;

            _timer -= Time.deltaTime;
            overlay.fillAmount = _timer /
                                 (_phase == Phase.Active ? activeSeconds : cooldownSeconds);

            if (_timer <= 0f)
            {
                if (_phase == Phase.Active)
                    BeginCooldown();
                else
                    BecomeReady();
            }
        }

        /* ──────────────── Phase changes ──────────────── */

        private void Activate()
        {
            if (_phase != Phase.Ready) return;

            // global event: start boost
            GameEvents.OnDoubleUpCoinsActivated?.Invoke();

            _phase = Phase.Active;
            _timer = activeSeconds;
            _btn.interactable = false;
            _img.sprite = disabledSprite;
            overlay.color = activeColor;
            overlay.fillAmount = 1f;
            overlay.gameObject.SetActive(true);
        }

        private void BeginCooldown()
        {
            // global event: end boost
            // GameEvents.OnCoinBoostEnded?.Invoke();

            _phase = Phase.Cooldown;
            _timer = cooldownSeconds;
            overlay.color = cooldownColor;
            overlay.fillAmount = 1f; // restart radial
        }

        private void BecomeReady()
        {
            _phase = Phase.Ready;
            overlay.gameObject.SetActive(false);
            _img.sprite = readySprite;
            _btn.interactable = true;
        }

        /* ──────────────── Tooltip (optional) ──────────────── */

        public void OnPointerEnter(PointerEventData _)
        {
            if (TooltipUI.Instance == null) return;

            string msg = _phase switch
            {
                Phase.Ready => $"Double income for {activeSeconds}s",
                Phase.Active => $"Boost active: {Mathf.CeilToInt(_timer)}s left",
                Phase.Cooldown => $"Cooldown: {Mathf.CeilToInt(_timer)}s",
                _ => ""
            };
            // TooltipUI.Instance.Show(msg, transform.position);

            // position directly above button
            Canvas canvas = TooltipUI.Instance.GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera
                ? canvas.worldCamera
                : null;

            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(cam, _rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPt,
                cam,
                out Vector2 localPt);

            float yOffset = _rect.rect.height * 0.7f; // above the button
            Vector2 tooltipPos = localPt + new Vector2(0f, yOffset);
            TooltipUI.Instance.Show(msg, tooltipPos);
        }

        public void OnPointerExit(PointerEventData _) => TooltipUI.Instance?.Hide();
    }
}