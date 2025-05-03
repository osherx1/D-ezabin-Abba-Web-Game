using UnityEngine;

namespace Utilities
{
    public class MoneyManager : MonoSingleton<MoneyManager>
    {
        [SerializeField] private int startingMoney = 100;

        public int CurrentMoney { get; private set; }

        private void Awake()
        {
            CurrentMoney = startingMoney;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney); 
        }

        public bool SpendMoney(int amount)
        {
            if (CurrentMoney < amount) return false;
            CurrentMoney -= amount;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney); 
            return true;
        }

        public void AddMoney(int amount)
        {
            CurrentMoney += amount;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney); 
        }
    }
}