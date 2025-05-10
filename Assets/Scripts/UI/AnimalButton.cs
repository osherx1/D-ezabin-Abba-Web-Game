using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utilities;

namespace UI
{
    /// <summary>
    /// Shop button for purchasing a creature.
    /// Supports four visual states:
    ///   0. Question    – unknown creature, not yet discovered
    ///   1. Silhouette  – preview of the next creature
    ///   2. Disabled    – unlocked but player cannot afford it
    ///   3. Enabled     – unlocked and affordable
    /// Displays a tooltip with the creature name and cost on hover/touch.
    /// </summary>
    [RequireComponent(typeof(Button), typeof(Image))]
    public class AnimalButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        /* ---------- Inspector ---------- */
        [Header("Cost & Prefab")] public int animalCost = 10;
        public GameObject animalPrefab;
        public string animalName = "Goat";

        [Header("Sprites")] public Sprite questionSprite; // Unknown
        public Sprite silhouetteSprite; // Preview
        public Sprite disabledSprite; // Unaffordable
        public Sprite enabledSprite; // Purchasable

        /* ---------- Internal ---------- */
        private enum State
        {
            Question,
            Silhouette,
            Disabled,
            Enabled
        }

        private State _state = State.Question;

        private Button _button;
        private Image _image;
        private RectTransform _rect; // add this field

        /* ---------- MonoBehaviour ---------- */
        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _button.onClick.AddListener(BuyAnimal);
            _rect = GetComponent<RectTransform>(); // assign RectTransform
            RefreshVisual();
        }

        private void OnEnable()
        {
            GameEvents.OnMoneyChanged += HandleMoneyChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnMoneyChanged -= HandleMoneyChanged;
        }

        /* ---------- Public API ---------- */
        /// <summary>Replaces the question mark with a silhouette sprite.</summary>
        public void RevealSilhouette()
        {
            if (_state == State.Question)
            {
                _state = State.Silhouette;
                RefreshVisual();
            }
        }

        /// <summary>Unlocks the button for purchase (called by ShopUIManager).</summary>
        public void Unlock()
        {
            _state = CanAfford() ? State.Enabled : State.Disabled;
            RefreshVisual();
        }

        /* ---------- Event Handlers ---------- */
        private void HandleMoneyChanged(float _)
        {
            if (_state != State.Disabled && _state != State.Enabled) return;

            _state = CanAfford() ? State.Enabled : State.Disabled;
            RefreshVisual();
        }

        private void BuyAnimal()
        {
            if (_state != State.Enabled) return;

            if (MoneyManager.Instance.SpendMoney(animalCost))
                CreatureSpawner.Instance.Spawn(animalPrefab);
        }

        /* ---------- UI Helpers ---------- */
        private bool CanAfford() =>
            MoneyManager.Instance.CurrentMoney >= animalCost;

        private void RefreshVisual()
        {
            switch (_state)
            {
                case State.Question:
                    _image.sprite = questionSprite;
                    _button.interactable = false;
                    break;
                case State.Silhouette:
                    _image.sprite = silhouetteSprite;
                    _button.interactable = false;
                    break;
                case State.Disabled:
                    _image.sprite = disabledSprite;
                    _button.interactable = false;
                    break;
                case State.Enabled:
                    _image.sprite = enabledSprite;
                    _button.interactable = true;
                    break;
            }
        }

        /* ---------- Tooltip ---------- */
        public void OnPointerEnter(PointerEventData e)
        {
            // if (_state == State.Question) return;
            // Ensure TooltipUI.Instance is valid
            if (TooltipUI.Instance == null) return;

            // Get the canvas and ensure it is valid
            var canvas = TooltipUI.Instance.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Determine the camera based on render mode
            var cam = canvas.renderMode == RenderMode.ScreenSpaceCamera
                ? canvas.worldCamera
                : null; // overlay uses null

            // Convert button center to screen point
            var screenPoint = RectTransformUtility.WorldToScreenPoint(cam, _rect.position);

            // Ensure canvas.transform is a RectTransform
            var rectTransform = canvas.transform as RectTransform;
            if (rectTransform == null) return;

            // Convert screen point to local point in canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPoint,
                cam,
                out Vector2 localPoint);

            // Offset tooltip position by 60% of button height
            var yOffset = _rect != null && _rect.rect.height > 0 ? _rect.rect.height * 0.6f : 0f;
            var tooltipPos = localPoint + new Vector2(0f, yOffset);
            CreatureCore creatureCore = animalPrefab.GetComponent<CreatureCore>();
            if (creatureCore == null) return;
            
            // Show the tooltip
            TooltipUI.Instance.Show($"{((int)animalCost).ToString("N0")} zuz\n +{creatureCore.zuzPerSecond} zps", tooltipPos);
            // TooltipUI.Instance.Show($"{animalCost}\n{animal} zuz", tooltipPos);
        }

        public void OnPointerExit(PointerEventData _) => TooltipUI.Instance.Hide();
        public void OnPointerDown(PointerEventData _) => TooltipUI.Instance.Hide();
    }
}