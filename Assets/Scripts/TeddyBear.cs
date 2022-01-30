using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SC
{
    public class TeddyBear : MonoBehaviour
    {
        public BoxCollider2D boxCollider;

        public void TurnOffCollider() => boxCollider.isTrigger = true;
    }
}
