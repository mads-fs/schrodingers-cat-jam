using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SC
{
    [Serializable, CreateAssetMenu]
    public class DialogueChain : ScriptableObject
    {
        public List<Dialogue> DialogueList = new List<Dialogue>();
    }
}
