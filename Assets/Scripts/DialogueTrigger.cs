using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SC
{
    public class DialogueTrigger : MonoBehaviour
    {
        public List<DialoguePair> Dialogues;

        /// <summary>
        /// Call to use with a provided world state.
        /// </summary>
        /// <param name="worldState">The world state to play a matching chain from.</param>
        public DialogueChain GetDialogue(int worldState)
        {
            if(worldState == -1) worldState = GameManager.Instance.WorldState;
            DialoguePair pair = Dialogues.FirstOrDefault(pred => pred.WorldStateTrigger == worldState);
            if (pair != default)
            {
                return pair.Chain;
            }
            else
            {
                return null;
            }
        }
    }

    [Serializable]
    public class DialoguePair
    {
        public int WorldStateTrigger;
        public DialogueChain Chain;
    }
}