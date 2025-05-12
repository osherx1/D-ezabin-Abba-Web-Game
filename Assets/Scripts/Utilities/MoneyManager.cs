using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities
{
    public class MoneyManager : MonoSingleton<MoneyManager>
    {
        [Header("Starting Settings")]
        [SerializeField] private int startingMoney    = 100;
        [SerializeField] private int badEndSceneIndex = 2;

        [Header("Income Settings")]
        [Tooltip("This is the *base* income from all creatures; boosted by boostMultiplier when active.")]
        [SerializeField] private float baseIncomePerSecond = 0f;
        [SerializeField] private float boostMultiplier     = 2f;

        public float IncomePerSecond => baseIncomePerSecond * _incomeMultiplier;
        public float CurrentMoney    { get; private set; }

        private float _incomeMultiplier = 1f;
        private float _boostTimer       = 0f;
        private float _tickTimer        = 0f;

        private void Awake()
        {
            CurrentMoney = startingMoney;
            // Fire initial events
            GameEvents.OnMoneyChanged?.Invoke(CurrentMoney);
            GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);
        }

        private void OnEnable()
        {
            GameEvents.OnCoinBoost += HandleBoost;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinBoost -= HandleBoost;
        }

        private void Update()
        {
            UpdateBoostTimer();
            TickIncome();
        }

        // ─────── Boost Handling ───────
        private void HandleBoost(float duration)
        {
            // if already boosted, ignore
            if (_boostTimer > 0f) return;

            _incomeMultiplier = boostMultiplier;
            GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);

            _boostTimer = duration;
        }

        private void UpdateBoostTimer()
        {
            if (_boostTimer <= 0f) return;

            _boostTimer -= Time.deltaTime;
            if (_boostTimer <= 0f)
            {
                // boost ended
                _incomeMultiplier = 1f;
                GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);
            }
        }

        // ─────── Passive Income Tick ───────
        private void TickIncome()
        {
            // if income is zero or negative, bad end
            if (IncomePerSecond <= 0f)
            {
                SceneManager.LoadScene(badEndSceneIndex);
                return;
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer < 1f) return;

            AddMoney(IncomePerSecond);
            _tickTimer = 0f;
        }

        // ─────── Public API ───────

        /// <summary>
        /// Called by creatures when they spawn or merge in.
        /// </summary>
        public void RegisterIncome(float amt)
        {
            baseIncomePerSecond += amt;
            GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);
        }

        /// <summary>
        /// Called by creatures when they die or merge out.
        /// </summary>
        public void UnregisterIncome(float amt)
        {
            baseIncomePerSecond -= amt;
            GameEvents.OnIncomeChanged?.Invoke(IncomePerSecond);
        }

        /// <summary>
        /// Attempt to spend—returns false if not enough.
        /// </summary>
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
