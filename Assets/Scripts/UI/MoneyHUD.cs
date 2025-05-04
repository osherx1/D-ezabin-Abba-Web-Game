using TMPro;
using UnityEngine;
using Utilities;

namespace UI
{
    public class MoneyHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text incomeText;

        private void OnEnable()
        {
            GameEvents.OnMoneyChanged += UpdateMoney;
            GameEvents.OnIncomeChanged += UpdateIncome;

            UpdateMoney(MoneyManager.Instance.CurrentMoney);
            UpdateIncome(MoneyManager.Instance.IncomePerSecond);
        }

        private void OnDisable()
        {
            GameEvents.OnMoneyChanged -= UpdateMoney;
            GameEvents.OnIncomeChanged -= UpdateIncome;
        }

        private void UpdateMoney(float value)
        {
            moneyText.text = ((int)value).ToString("N0"); // למשל: 1,234
        }

        private void UpdateIncome(float value)
        {
            incomeText.text = $"+{value}/zps";
        }
    }
}