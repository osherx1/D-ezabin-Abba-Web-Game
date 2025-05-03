using System;
using UnityEngine;

namespace Utilities
{
    public class GameEvents : MonoBehaviour
    {
        public static Action StartScytheAttack;
        public static Action StartMinionsAttack;
        public static Action<int> OnMoneyChanged; 

    }
}
