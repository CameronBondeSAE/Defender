using System;
using UnityEngine;

namespace AshleyPearson
{
    public class ScoutEvents : MonoBehaviour
    {
        public static Action OnScoutReady;
        public static Action OnNoInformationFound;
        public static Action<int> OnInformationToReport;
        public static Action<Transform> OnFoundPlayer;
        public static Action OnReport;

    }
}