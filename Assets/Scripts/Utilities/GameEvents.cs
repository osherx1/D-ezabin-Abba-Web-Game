using System;
using UnityEngine;

namespace Utilities
{
    public class GameEvents : MonoBehaviour
    {
        public static Action RestartGame;
        public static Action StartSwipingAttack;
        public static Action StartMinionsAttack;
        public static Action OnCreaturePurchased;
        public static Action<float> OnMoneyChanged;
        public static Action<float> OnIncomeChanged;
        public static Action<CreatureStage> OnCreatureMerged;
        public static Action MuteSounds;
        public static Action<string> StopSoundByName;
        public static Action PauseUnpauseBackgroundMusic;
        public static Action FreezeEnemies;
        public static Action OnOxButcherMerged;
        public static Action OnFinalCreatureAnimationEnded;
        public static Action StopAllEnemies;
        public static Action DestroyAllEnemies;
        public static Action<float> OnShieldActivated;
        public static Action OnSwordActivated;
        public static Action<float> OnCoinBoost;
    }
}