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
        public UnityEvent OnInteract;

        private void Start() => GetComponent<BoxCollider2D>().isTrigger = true;
    }
}
