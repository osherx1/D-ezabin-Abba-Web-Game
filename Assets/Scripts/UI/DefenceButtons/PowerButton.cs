using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Utilities;

namespace UI
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
            TooltipUI.Instance?.Hide();
        }

        private void UpdateTooltip()
        {
            if (TooltipUI.Instance == null) return;

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
            var canvas = TooltipUI.Instance.GetComponentInParent<Canvas>();
            var cam = canvas.renderMode == RenderMode.ScreenSpaceCamera
                ? canvas.worldCamera
                : null;
            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(cam, _rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPt, cam, out Vector2 localPt);

            float yOff = _rect.rect.height * 0.7f;
            TooltipUI.Instance.Show(msg, localPt + new Vector2(0f, yOff));
        }

        /* ───────── Helper ───────── */
        private void FireEvent(bool isStart)
        {
            switch (powerType)
            {
                case PowerType.Coin:
                    if (isStart) GameEvents.OnDoubleUpCoinsActivated?.Invoke();
                    break;
                case PowerType.Shield:
                    if (isStart) GameEvents.OnShieldActivated?.Invoke();
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