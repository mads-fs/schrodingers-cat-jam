using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SC
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Interactable : MonoBehaviour
    {
        public event EventHandler OnHitFloor;

        public bool SingleUse = false;
        public bool ListenForHitFloor = false;
        public UnityEvent OnInteract;
        private BoxCollider2D _trigger;

        private void Start()
        {
            _trigger = GetComponent<BoxCollider2D>();
            _trigger.isTrigger = true;
        }

        public void PushSelf(float force)
        {
            Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
            CatController player = FindObjectOfType<CatController>();
            if (player.transform.position.x < transform.position.x)
            {
                // to the right of object
                body.AddForce(new Vector2(force, force));
            }
            else
            {
                // to the left of object
                body.AddForce(new Vector2(-force, force));
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (ListenForHitFloor == true)
            {
                if (collision.gameObject.tag == "Floor")
                {
                    OnHitFloor?.Invoke(this, null);
                }
            }
        }
    }
}
