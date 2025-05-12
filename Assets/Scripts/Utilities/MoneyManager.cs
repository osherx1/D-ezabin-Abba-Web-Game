using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities
{
    public class MoneyManager : MonoSingleton<MoneyManager>
    {
        [SerializeField] private int   startingMoney   = 100;
        [SerializeField] private int   badEndSceneIndex = 2;
        [SerializeField] private float incomePerSecond;
        [SerializeField] private float boostMultiplier = 2f;

        public  float IncomePerSecond => incomePerSecond;
        public  float CurrentMoney    { get; private set; }

        private float _boostTimer = 0f;         // >0 while boost is active
        private float _originalIncome;
        private float _tickTimer = 0f;

        /* ─────────── MonoBehaviour ─────────── */
        private void Awake()
        {
            CurrentMoney = startingMoney;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney);
            GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);
        }

        private void OnEnable()  => GameEvents.OnCoinBoost += HandleBoost;
        private void OnDisable() => GameEvents.OnCoinBoost -= HandleBoost;

        private void Update()
        {
            UpdateBoostTimer();
            TickIncome();
        }

        /* ─────────── Boost handling ─────────── */
        private void HandleBoost(float duration)
        {
            // ignore if already boosted and multiplier is still applied
            if (_boostTimer > 0f) return;

            _originalIncome  = incomePerSecond;
            incomePerSecond *= boostMultiplier;
            GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);

            _boostTimer = duration;             // start local timer
        }

        private void UpdateBoostTimer()
        {
            if (_boostTimer <= 0f) return;

            _boostTimer -= Time.deltaTime;
            if (_boostTimer <= 0f)
            {
                incomePerSecond = _originalIncome;
                GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);
            }
        }

        /* ─────────── Passive income tick ─────────── */
        private void TickIncome()
        {
            if (incomePerSecond <= 0)
            {
                SceneManager.LoadScene(badEndSceneIndex);
                return;
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer < 1f) return;

            AddMoney(incomePerSecond);
            _tickTimer = 0f;
        }

        /* ─────────── Helpers ─────────── */
        public void RegisterIncome(float amt)
        {
            incomePerSecond += amt;
            GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);
        }

        public void UnregisterIncome(float amt)
        {
            incomePerSecond -= amt;
            GameEvents.OnIncomeChanged?.Invoke(incomePerSecond);
        }

        public bool SpendMoney(float amt)
        {
            if (CurrentMoney < amt) return false;
            CurrentMoney -= amt;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney);
            return true;
        }

        private void AddMoney(float amt)
        {
            CurrentMoney += amt;
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney);
        }
    }
}