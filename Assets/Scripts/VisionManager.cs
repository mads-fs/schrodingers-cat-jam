using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    public class VisionManager : MonoBehaviour
    {
        public List<GameObject> CurrentlySeen;
        private SpriteRenderer _playerRenderer;

        private void Start()
        {
            _playerRenderer = FindObjectOfType<CatController>().gameObject.GetComponentInChildren<SpriteRenderer>();
            CurrentlySeen = new List<GameObject>();
        }

        private void Update()
        {
            bool isRight = _playerRenderer.flipX;
            Vector3 offset;
            if (isRight == false) offset = new Vector3(0.5f, 0, 0);
            else offset = new Vector3(-0.5f, 0, 0);
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position + offset, new Vector2(1, 1), 0);
            CurrentlySeen.Clear();
            if (hits.Length > 0)
            {
                foreach (Collider2D collider in hits)
                {
                    Interactable interactable = collider.gameObject.GetComponent<Interactable>();
                    if (interactable) CurrentlySeen.Add(interactable.gameObject);
                }
            }
        }
    }

}