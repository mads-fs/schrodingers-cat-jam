using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SC
{
    [Serializable, CreateAssetMenu]
    public class DialogueChain : ScriptableObject
    {
        public int NewWorldState = -1;
        public List<Dialogue> DialogueList = new List<Dialogue>();
    }
}
