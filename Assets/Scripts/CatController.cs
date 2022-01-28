using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    public class CatController : MonoBehaviour
    {
        public event EventHandler OnAlived;
        public event EventHandler OnUnalived;

        private void LeaveSpiritRealm() => OnAlived?.Invoke(this, null);
        private void EnterSpiritRealm() => OnUnalived?.Invoke(this, null);
    }

}