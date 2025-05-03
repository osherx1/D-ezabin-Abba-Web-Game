using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class AnimalButton : MonoBehaviour
{
    [Header("Cost & Prefab")]
    public int animalCost = 10;
    public GameObject animalPrefab;

    [Header("Sprites")]
    public Sprite lockedSprite;
    public Sprite disabledSprite;  
    public Sprite enabledSprite;

    private bool _unlocked = false;
    private Button _button;
    private Image  _image;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image  = GetComponent<Image>();
        _button.onClick.AddListener(BuyAnimal);
    }

    private void OnEnable()
    {
        GameEvents.OnMoneyChanged += HandleMoneyChanged;
        SetVisualState();
    }

    private void OnDisable()
    {
        GameEvents.OnMoneyChanged -= HandleMoneyChanged;
    }

    public void Unlock()
    {
        _unlocked = true;
        SetVisualState();
    }

    private void HandleMoneyChanged(int _)
    {
        if (_unlocked) SetVisualState();
    }

    private void SetVisualState()
    {
        if (!_unlocked)
        {
            _button.interactable = false;
            _image.sprite = lockedSprite;
            return;
        }

        var canAfford = MoneyManager.Instance.CurrentMoney >= animalCost;
        _button.interactable = canAfford;
        _image.sprite = canAfford ? enabledSprite : disabledSprite;
    }

    private void BuyAnimal()
    {
        if (!_unlocked) return;

        if (MoneyManager.Instance.SpendMoney(animalCost))
            CreatureSpawner.Instance.Spawn(animalPrefab);
        else
            Debug.Log("Not enough money!");
    }
}