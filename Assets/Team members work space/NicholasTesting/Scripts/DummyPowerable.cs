using UnityEngine;

namespace NicholasScripts
{
    public class DummyPowerable : MonoBehaviour, IPowerable
    {
        public void SetPowered(bool state)
        {
            Debug.Log($"{gameObject.name} was powered: {state}");
            GetComponent<Renderer>().material.color = state ? Color.green : Color.red;
        }

        private void Start()
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
    }
}