using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class AnimalButton : MonoBehaviour
{
    public int animalCost = 10;
    public GameObject animalPrefab; // The prefab to spawn

    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(BuyAnimal);
    }

    private void BuyAnimal()
    {
        if (MoneyManager.Instance.SpendMoney(animalCost))
        {
            CreatureSpawner.Instance.Spawn(animalPrefab);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }
}