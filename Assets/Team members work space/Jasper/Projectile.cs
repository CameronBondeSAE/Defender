using System;
using UnityEngine;

namespace Jasper_AI
{
    public class Projectile : MonoBehaviour
    {
        public bool tracking;
        public GameObject target;
        public int strength;
        public int speed; 

        public void SetValues(GameObject targetObject, int strengthValue, bool track = false)
        {
            target = targetObject;
            strength = strengthValue;
            tracking = track;
        }
        
        private void Update()
        {
            transform.position += transform.forward * (Time.deltaTime * speed);
        }
    }
}
