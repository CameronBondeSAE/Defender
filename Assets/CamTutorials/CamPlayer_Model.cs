using UnityEngine;

namespace Tutorials
{
    public class CamPlayer_Model : MonoBehaviour
    {
        public CamPlayer_Controller CamPlayerController;
        public Rigidbody rb;
        public float speed = 100f;

        [SerializeField] private float jumpForce = 100f;

        private void FixedUpdate()
        {
            rb.AddForce(CamPlayerController.direction * speed);
        }

        public void Jump()
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
}