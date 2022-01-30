using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SC
{
    [Serializable, CreateAssetMenu]
    public class Dialogue : ScriptableObject
    {
        [TextArea]
        public string Content;
    }
}
