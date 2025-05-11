using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


namespace Utilities
{
    public class MoneyManager : MonoSingleton<MoneyManager>
    {
        [SerializeField] private int startingMoney = 100;
        [SerializeField] private int badEndSceneIndex = 2;
        [SerializeField] private float incomePerSecond;
        public float IncomePerSecond => incomePerSecond;

        public float CurrentMoney { get; private set; }

        /* ---------- Public API ---------- */
        public void RegisterIncome(float amount)
        {
            incomePerSecond += amount;
            GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);
        }

        public void UnregisterIncome(float amount)
        {
            incomePerSecond -= amount;
            GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);
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
            GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);
        }

        private float _timer;

        private void Update()
        {
            if (incomePerSecond <= 0)
            {
                SceneManager.LoadScene(badEndSceneIndex);
                return;
            }

            ;

            _timer += Time.deltaTime;
            if (!(_timer >= 1f)) return;
            AddMoney(incomePerSecond);
            _timer = 0f;
        }
    }
}