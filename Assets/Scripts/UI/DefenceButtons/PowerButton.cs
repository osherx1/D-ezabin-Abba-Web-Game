using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace UI.DefenceButtons
{
    public enum PowerType
    {
        Coin,
        Shield,
        Sword
    }

    [RequireComponent(typeof(Button), typeof(Image))]
    public class PowerButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        /* ───────── Inspector ───────── */
        [Header("Identify")] [SerializeField] private PowerType powerType = PowerType.Coin;

        [Header("Sprites")] [SerializeField] private Sprite readySprite;
        [SerializeField] private Sprite disabledSprite;

        [Header("Overlay (Filled/Radial)")] [SerializeField]
        private Image overlayImage;

        // [SerializeField] private TMP_Text timerLabel;

        [Header("Timing")] [SerializeField] private float activeSeconds = 10f;
        [SerializeField] private float cooldownSeconds = 60f;

        [Header("Overlay Colours")] [SerializeField]
        private Color activeColor = new(1f, 0.9f, 0f, 0.65f);

        [SerializeField] private Color cooldownColor = new(0f, 0f, 0f, 0.55f);

        /* ───────── Runtime ───────── */
        private enum Phase
        {
            Ready,
            Active,
            Cooldown
        }

        private Phase _phase = Phase.Ready;
        private Button _btn;
        private Image _img;
        private RectTransform _rect;
        private float _timer;
        private bool _hover;

        /* ───────── Unity ───────── */
        private void Awake()
        {
            _btn = GetComponent<Button>();
            _img = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            _btn.onClick.AddListener(OnPressed);

            overlayImage.gameObject.SetActive(false);
            // timerLabel.gameObject.SetActive(false);
            _img.sprite = readySprite;
        }

        private void Update()
        {
            if (_phase == Phase.Ready) return;

            _timer -= Time.deltaTime;
            float total = _phase == Phase.Active ? activeSeconds : cooldownSeconds;
            overlayImage.fillAmount = _timer / total;
            // timerLabel.text = Mathf.CeilToInt(_timer).ToString();

            if (_hover) UpdateTooltip();

            if (_timer <= 0f)
            {
                if (_phase == Phase.Active) BeginCooldown();
                else BecomeReady();
            }
        }

        /* ───────── Button logic ───────── */
        private void OnPressed()
        {
            if (_phase != Phase.Ready) return;

            FireEvent(true); // start event

            _phase = Phase.Active;
            _timer = activeSeconds;

            _btn.interactable = false;
            _img.sprite = disabledSprite;

            overlayImage.color = activeColor;
            overlayImage.fillAmount = 1f;
            overlayImage.gameObject.SetActive(true);
            // timerLabel.gameObject.SetActive(true);
        }

        private void BeginCooldown()
        {
            FireEvent(false); // end event

            _phase = Phase.Cooldown;
            _timer = cooldownSeconds;

            overlayImage.color = cooldownColor;
            overlayImage.fillAmount = 1f;
        }

        private void BecomeReady()
        {
            _phase = Phase.Ready;

            overlayImage.gameObject.SetActive(false);
            // timerLabel.gameObject.SetActive(false);

            _img.sprite = readySprite;
            _btn.interactable = true;
        }

        /* ───────── Tooltip ───────── */
        public void OnPointerEnter(PointerEventData _)
        {
            _hover = true;
            UpdateTooltip();
        }

        public void OnPointerExit(PointerEventData _)
        {
            _hover = false;
            PowerTooltip.Instance?.Hide();
        }

        private void UpdateTooltip()
        {
            if (PowerTooltip.Instance == null) return;

            string msg = _phase switch
            {
                Phase.Ready => powerType switch
                {
                    PowerType.Coin => $"Double income for {activeSeconds}s",
                    PowerType.Shield => $"Activate shield for {activeSeconds}s",
                    PowerType.Sword => $"Increase attack for {activeSeconds}s",
                    _ => ""
                },
                Phase.Active => $"Active: {Mathf.CeilToInt(_timer)}s left",
                Phase.Cooldown => $"Cooldown: {Mathf.CeilToInt(_timer)}s",
                _ => ""
            };

            // position above button
            var canvas = PowerTooltip.Instance.GetComponentInParent<Canvas>();
            var cam = canvas.renderMode == RenderMode.ScreenSpaceCamera
                ? canvas.worldCamera
                : null;
            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(cam, _rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPt, cam, out Vector2 localPt);

            float xOff = _rect.rect.width * 0.7f;
            PowerTooltip.Instance.Show(msg, localPt + new Vector2(-xOff, 0));
        }

        /* ───────── Helper ───────── */
        private void FireEvent(bool isStart)
        {
            switch (powerType)
            {
                case PowerType.Coin:
                    if (isStart) GameEvents.OnCoinBoost?.Invoke(activeSeconds);
                    break;
                case PowerType.Shield:
                    if (isStart) GameEvents.OnShieldActivated?.Invoke(activeSeconds);
                    break;
                case PowerType.Sword:
                    if (isStart) GameEvents.OnSwordActivated?.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}