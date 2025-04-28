namespace Utilities
{
    public class MoneyManager : MonoSingleton<MoneyManager>
    {
        private int _currentMoney = 100; // starting money

        public bool SpendMoney(int amount)
        {
            if (_currentMoney >= amount)
            {
                _currentMoney -= amount;
                return true;
            }

            return false;
        }

        public void AddMoney(int amount)
        {
            _currentMoney += amount;
        }
    }
}