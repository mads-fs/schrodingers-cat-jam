using System;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    [Serializable, CreateAssetMenu]
    public class DialogueChain : ScriptableObject
    {
        public List<Dialogue> DialogueList;
    }
}
