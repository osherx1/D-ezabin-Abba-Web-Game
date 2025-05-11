using System;
using UnityEngine;

namespace Utilities
{
    public class GameEvents : MonoBehaviour
    {
        public static Action StartSwipingAttack;
        public static Action StartMinionsAttack;
        public static Action<float> OnMoneyChanged;
        public static Action<float> OnIncomeChanged;
        public static Action<CreatureStage> OnCreatureMerged;
        public static Action MuteSounds;
        public static Action<string> StopSound;
        public static Action PauseUnpauseBackgroundMusic;
        public static Action FreezeEnemies;
        public static Action OnOxButcherMerged;
    }
}
