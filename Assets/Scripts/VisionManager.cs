using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    public class VisionManager : MonoBehaviour
    {
        public List<GameObject> CurrentlySeen;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.GetComponent<Interactable>()) CurrentlySeen.Add(collision.gameObject);
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponent<Interactable>()) CurrentlySeen.Remove(other.gameObject);
        }
    }

}