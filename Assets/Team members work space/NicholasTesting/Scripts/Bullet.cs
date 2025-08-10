using UnityEngine;

namespace NicholasScripts
{
    public class Bullet : MonoBehaviour
    {
        public float speed = 20f;
        public float lifetime = 5f;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}