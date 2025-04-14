using UnityEngine;
using UnityEngine.Events;

namespace CWM.Skinn.Gameplay
{
    public class ActivationEvent : MonoBehaviour
    {
        [ComponentHeader]
        public string header = "";
        [System.Serializable]
        public class ActiveEvent : UnityEvent<bool> { }

        public bool flip;

        [Space]

        public ActiveEvent active;

        [Space]

        public UnityEvent onStart;
        public UnityEvent onEnabled;
        public UnityEvent onDisabled;
        public UnityEvent onDestroy;

        public void Start() { onStart.Invoke(); }

        public void OnEnable()
        {
            if (!flip) active.Invoke(true);
            else active.Invoke(false);

            onEnabled.Invoke();
        }

        public void OnDisable()
        {
            if (!flip) active.Invoke(false);
            else active.Invoke(true);

            onDisabled.Invoke();
        }

        public void OnDestroy() { onDestroy.Invoke(); }
    }
}
