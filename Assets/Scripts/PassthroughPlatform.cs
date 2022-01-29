using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PassthroughPlatform : MonoBehaviour
    {
        private CatController _player;
        private BoxCollider2D _collider;
        private void Start()
        {
            _player = FindObjectOfType<CatController>();
            _collider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (_player.transform.position.y > (transform.position.y - (_player.minGroundNormalY * 0.5f)))
            {
                _collider.enabled = true;
            }
            else
            {
                _collider.enabled = false;
            }
        }
    }
}