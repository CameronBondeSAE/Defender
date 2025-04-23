using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace AIEvents
{
    public class AIEventManager : MonoBehaviour
    {
        public static AIEventManager instance;

        public AIEvents events = new AIEvents();
        [HideInInspector] public bool isReady = false;

        void OnEnable()
        {
            instance = this;
            events = new AIEvents();
            // subscribing event handlers to AI state events
            events.onDeath.AddListener(OnDeath);
            events.onAttack.AddListener(OnAttack);
            events.onSearch.AddListener(OnAlert);
            events.onPatrol.AddListener(OnPatrol);
            events.onHit.AddListener(OnHit);
        }

        void OnDisable()
        {
            // unsubscribing
            events.onDeath.RemoveListener(OnDeath);
            events.onAttack.RemoveListener(OnAttack);
            events.onSearch.RemoveListener(OnAlert);
            events.onPatrol.RemoveListener(OnPatrol);
            events.onHit.RemoveListener(OnHit);
        }

        void Start()
        {
            // marks this event manager as ready to process events
            isReady = true;
        }

        private void OnDeath(AIBase ai)
        {
        }

        private void OnAttack(AIBase ai)
        {
        }

        private void OnParanoid(AIBase ai)
        {
        }

        private void OnHit(AIBase ai)
        {
            Debug.Log($"{ai.name} is hit.");
        }

        private void OnAlert(AIBase ai)
        {

        }

        private void OnPatrol(AIBase ai)
        {

        }


    }

    // stores all AI related events, which can be subscribed to by different scripts and components
    public class AIEvents
    {
        // AI State Events
        public OnAIEvent onAttack = new OnAIEvent();
        public OnAIEvent onPatrol = new OnAIEvent();
        public OnAIEvent onSearch = new OnAIEvent();
        public OnAIEvent onHit = new OnAIEvent();
        public OnAIEvent onDeath = new OnAIEvent();

        public OnAttackActionEvent onAttackAction = new OnAttackActionEvent();
    }

    // custom unity event types that pass an AI instance as a parameter
    [System.Serializable]
    public class OnAIEvent : UnityEvent<AIBase>
    {
    }

    [System.Serializable]
    public class OnDeathEvent : UnityEvent<AIBase>
    {
    }

    [System.Serializable]
    public class OnAttackActionEvent : UnityEvent<AIBase>
    {
    }
}
