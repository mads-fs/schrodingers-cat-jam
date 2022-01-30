using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SC
{
    public class ChangeByHittingFloor : MonoBehaviour
    {
        public Sprite NewSprite;
        public UnityEvent OnHittingFloor;

        private void Start()
        {
            GetComponent<Interactable>().OnHitFloor += Interactable_OnHitFloor;
        }

        private void Interactable_OnHitFloor(object sender, EventArgs e)
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            GetComponent<SpriteRenderer>().sprite = NewSprite;
            Destroy(GetComponent<Interactable>());
            Destroy(GetComponent<Rigidbody2D>());
            OnHittingFloor?.Invoke();
        }
    }
}
