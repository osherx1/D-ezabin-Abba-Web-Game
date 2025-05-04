using UnityEngine;

namespace Utilities
{
    public class MoneyManager : MonoSingleton<MoneyManager>
    {
        [SerializeField] private int startingMoney = 100;


        public float CurrentMoney { get; private set; }
        public float IncomePerSecond { get; private set; }

        /* ---------- Public API ---------- */
        public void RegisterIncome(float amount) 
        {
            IncomePerSecond += amount;
            GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);
        }

        public void UnregisterIncome(float amount) 
        {
            IncomePerSecond -= amount;
            GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);
        }

        public bool SpendMoney(float amount)
        {
            if (CurrentMoney < amount) return false;
            CurrentMoney -= amount;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney);
            return true;
        }

        private void AddMoney(float amount)
        {
            CurrentMoney += amount;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney);
        }

        /* ---------- MonoBehaviour ---------- */
        private void Awake()
        {
            CurrentMoney = startingMoney;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney);
            GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);
        }

        private float _timer;

        private void Update()
        {
            if (IncomePerSecond == 0) return;

            _timer += Time.deltaTime;
            if (!(_timer >= 1f)) return;
            AddMoney(IncomePerSecond);
            _timer = 0f;
        }
    }
}